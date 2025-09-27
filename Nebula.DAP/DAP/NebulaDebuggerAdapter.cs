using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
using Nebula.Commons.Debugger;
using Nebula.Debugger.Debugger;
using Nebula.Debugger.Debugger.Data;
using Nebula.Interop.SafeHandles;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nebula.Debugger.DAP
{
    public sealed class NebulaDebuggerAdapter
        : DebugAdapterBase
    {
        public DebuggerConfiguration Configuration { get; } = new();

        private readonly ILogger _logger;
        private readonly NebulaDebugger _debugger;
        private readonly Dictionary<string, Source> _dapSources = [];
        // Hold delegates in memory otherwise native bindings will explode!
        private readonly StdOutEventHandler _writeDelegate;
        private readonly StdOutEventHandler _writeLineDelegate;
        private readonly ExitEventHandler _exitDelegate;

        private readonly System.Threading.Tasks.Task _debuggerDispatchTask;
        private readonly System.Threading.CancellationTokenSource _tokenSource = new();
        private readonly BlockingCollection<EventInfo> _debugEvents = [];
        private readonly System.Threading.ManualResetEvent _queueNotProcessingEvent = new(true);
        private void DebuggerDispatchTask()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                EventInfo nextEvent = _debugEvents.Take(_tokenSource.Token);
                if (_tokenSource.IsCancellationRequested)
                {
                    return;
                }

                if (_debugger.VirtualMachineFinishedExecution)
                {
                    return;
                }

                _queueNotProcessingEvent.Reset();

                try
                {
                    switch (nextEvent.Type)
                    {
                        case EventType.Step:
                            {
                                _debugger.AbortStepping = false;
                                if (_debugger.StepLine(nextEvent.ThreadId, out HitBreakpointInformation? hitBreakpoint))
                                {
                                    if (hitBreakpoint != null)
                                    {
                                        SendStoppedEvent(hitBreakpoint.IsFunctionBreakpoint ? StoppedEvent.ReasonValue.FunctionBreakpoint : StoppedEvent.ReasonValue.Breakpoint,
                                                         hitBreakpoint.ThreadId);
                                    }
                                    else
                                    {
                                        SendStoppedEvent(StoppedEvent.ReasonValue.Step, nextEvent.ThreadId);
                                    }
                                }
                                break;
                            }
                        case EventType.StepIn:
                            {
                                _debugger.AbortStepping = false;
                                if (_debugger.StepIn(nextEvent.ThreadId, out HitBreakpointInformation? hitBreakpoint))
                                {
                                    if (hitBreakpoint != null)
                                    {
                                        SendStoppedEvent(hitBreakpoint.IsFunctionBreakpoint ? StoppedEvent.ReasonValue.FunctionBreakpoint : StoppedEvent.ReasonValue.Breakpoint,
                                                         hitBreakpoint.ThreadId);
                                    }
                                    else
                                    {
                                        SendStoppedEvent(StoppedEvent.ReasonValue.Step, nextEvent.ThreadId);
                                    }
                                }

                                break;
                            }
                        case EventType.Continue:
                            {
                                _debugger.AbortStepping = false;
                                if (_debugger.Continue(out HitBreakpointInformation? hitBreakpoint))
                                {
                                    if (hitBreakpoint != null)
                                    {
                                        SendStoppedEvent(hitBreakpoint.IsFunctionBreakpoint ? StoppedEvent.ReasonValue.FunctionBreakpoint : StoppedEvent.ReasonValue.Breakpoint,
                                                     hitBreakpoint.ThreadId);
                                    }

                                    // Is there any other case?
                                }
                                break;
                            }
                        case EventType.Unknown:
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception while processing debugger event '{nextEvent.Type}': {ex}");
                }

                _queueNotProcessingEvent.Set();
            }
        }

        #region To Client Events
        private void SendInitializedEvent()
        {
            Protocol.SendEvent(new InitializedEvent());
        }

        private void SendStoppedEvent(StoppedEvent.ReasonValue reason, int? threadId)
        {
            _debugger.ReloadVirtualMachineState();
            Protocol.SendEvent(new StoppedEvent(reason)
            {
                ThreadId = threadId,
                AllThreadsStopped = true,
            });
        }

        private void SendTerminationEvent()
        {
            _debugger.AbortStepping = true;
            Protocol.SendEvent(new TerminatedEvent());
        }

        private void TerminateTask(bool waitForTask = true)
        {
            _debuggerDispatchTask.ContinueWith((t) => _debuggerDispatchTask.Dispose());
            _tokenSource.Cancel();
            if (waitForTask)
            {
                _debuggerDispatchTask.Wait();
            }
        }


        #endregion

        public NebulaDebuggerAdapter(Stream inStream, Stream outStream, ILogger logger)
        {
            _logger = logger;
            _writeDelegate = OnStdOutWrite;
            _writeLineDelegate = OnStdOutWriteLine;
            _exitDelegate = OnVirtualMachineExit;

            _debugger = new(_logger);
            _debugger.RedirectOutput(_writeDelegate, _writeLineDelegate);
            _debugger.SetExitCallback(_exitDelegate);
            _debuggerDispatchTask = System.Threading.Tasks.Task.Run(DebuggerDispatchTask, _tokenSource.Token);

            InitializeProtocolClient(inStream, outStream);
        }

        private void OnVirtualMachineExit()
        {
            SendTerminationEvent();
        }

        public void Run()
        {
            Protocol.Run();
        }

        #region Initialization
        protected override SetDebuggerPropertyResponse HandleSetDebuggerPropertyRequest(SetDebuggerPropertyArguments arguments)
        {
            return new SetDebuggerPropertyResponse();
        }

        protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments)
        {
            Configuration.PathType = arguments.PathFormat ?? InitializeArguments.PathFormatValue.Path;


            SendInitializedEvent();
            return new InitializeResponse()
            {
                SupportsFunctionBreakpoints = true,
                SupportsDebuggerProperties = true,
                SupportsSetVariable = true,
                SupportsConfigurationDoneRequest = true,
            };
        }

        protected override SetFunctionBreakpointsResponse HandleSetFunctionBreakpointsRequest(SetFunctionBreakpointsArguments arguments)
        {
            _debugger.BreakpointManager.ClearFunctionBreakpoints();
            List<Breakpoint> actualBreakpoints = [];

            foreach (FunctionBreakpoint? breakpoint in arguments.Breakpoints)
            {
                string requestedFunctionName = breakpoint.Name;
                Breakpoint bp = new()
                {
                    Verified = false,
                    Line = -1,
                    Source = null,
                    Reason = Breakpoint.ReasonValue.Failed
                };

                actualBreakpoints.Add(bp);
                string[] tokens = requestedFunctionName.Split("::", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (tokens.Length != 2)
                {
                    continue;
                }

                string breakpointNamespace = tokens[0];
                string breakPointFunctionName = tokens[1];

                if (!_debugger.DebugFiles.TryGetValue(breakpointNamespace, out DebugFile? dbgFile))
                {
                    _logger.LogWarning($"Namespace for function breakpoint '{breakpointNamespace}' not found in dbg files");
                    bp.Message = $"Debug symbols for namespace '{breakpointNamespace}' not found!";
                    continue;
                }

                if (!dbgFile.Functions.TryGetValue(breakPointFunctionName, out DebugFunction? dbgFunc))
                {
                    _logger.LogWarning($"Debug symbols of function '{breakPointFunctionName}' in namespace '{breakpointNamespace}' not found");
                    bp.Message = $"Debug symbols for namespace '{breakpointNamespace}' do not contain information about function '{breakPointFunctionName}'";
                    continue;
                }

                bp.Verified = true;
                bp.Line = dbgFunc.LineNumber;
                bp.Source = _dapSources[breakpointNamespace];
                bp.Reason = null;

                _debugger.BreakpointManager.AddFunctionBreakpoint(new BreakpointInformation(breakpointNamespace, breakPointFunctionName, -1));
            }

            return new()
            {
                Breakpoints = actualBreakpoints,
            };
        }

        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            _debugger.BreakpointManager.ClearBreakpoints();
            List<Breakpoint> actualBreakpoints = [];

            if (arguments.Source.SourceReference > 0)
            {
                _logger.LogError("Received source reference id but it is not supported yet!");
                SendTerminationEvent();
                return new();
            }

            if (!_dapSources.TryFirstOrDefault(s => s.Value.Path.Equals(arguments.Source.Path, StringComparison.InvariantCultureIgnoreCase),
                                               out KeyValuePair<string, Source> kvp))
            {
                _logger.LogInformation($"No source found for '{arguments.Source.Path}', cannot set breackpoints");

                foreach (SourceBreakpoint? reqBp in arguments.Breakpoints)
                {
                    actualBreakpoints.Add(new Breakpoint()
                    {
                        Verified = false,
                        Line = reqBp.Line,
                        Column = reqBp.Column,
                        Source = null,
                        Reason = Breakpoint.ReasonValue.Failed,
                    });
                }

                return new()
                {
                    Breakpoints = actualBreakpoints
                };
            }

            string @namespace = kvp.Key;
            Source source = kvp.Value;

            foreach (SourceBreakpoint? reqBp in arguments.Breakpoints)
            {
                Breakpoint resBp = new()
                {
                    Verified = false,
                    Line = reqBp.Line,
                    Column = reqBp.Column,
                    Source = source,
                    Reason = Breakpoint.ReasonValue.Failed,
                };

                actualBreakpoints.Add(resBp);

                if (!_debugger.IsLineDebuggable(@namespace, reqBp.Line, out string functionName, out int instructionIndex))
                {
                    resBp.Message = $"Line '{reqBp.Line}' is not debuggable";
                    continue;
                }

                resBp.Verified = true;
                _debugger.BreakpointManager.AddBreakpoint(new(@namespace, functionName, instructionIndex));
            }

            return new()
            {
                Breakpoints = actualBreakpoints,
            };
        }

        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            return new ConfigurationDoneResponse() { };
        }

        protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments)
        {
            Configuration.StepOnEntry = arguments.ConfigurationProperties.GetValueAsBool(DebuggerConstants.StepOnEntry) ?? false;
            Configuration.RecompileOnLaunch = arguments.ConfigurationProperties.GetValueAsBool(DebuggerConstants.RecompileOnLaunch) ?? false;
            Configuration.CompilerPath = arguments.ConfigurationProperties.GetValueAsString(DebuggerConstants.CompilerPath) ?? string.Empty;


            IEnumerable<string> scriptDirectories = FetchAllSourceDirectories(arguments);

            if (Configuration.RecompileOnLaunch &&
                File.Exists(Configuration.CompilerPath))
            {
                if (!RecompileScripts(scriptDirectories, Configuration.CompilerPath))
                {
                    SendTerminationEvent();
                    return new();
                }
            }

            if (!LoadNebulaScripts(scriptDirectories, arguments, Configuration))
            {
                SendTerminationEvent();
                return new();
            }

            _debugger.Initialize(Configuration.StepOnEntry);
            if (Configuration.StepOnEntry)
            {
                SendStoppedEvent(StoppedEvent.ReasonValue.Entry, 0);
            }

            return new LaunchResponse()
            {
                /* Nothing */
            };
        }

        private bool RecompileScripts(IEnumerable<string> scriptFolders, string compilerPath)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new(compilerPath);
            startInfo.FileName = compilerPath;
            startInfo.ArgumentList.Add("--next_to_source");
            foreach (string folder in scriptFolders)
            {
                startInfo.ArgumentList.Add($"-f {folder}");
                startInfo.ArgumentList.Add($"-r {folder}");
            }

            startInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process? process = System.Diagnostics.Process.Start(startInfo);

            if (process != null)
            {
                process.EnableRaisingEvents = true;
                string output = process.StandardOutput.ReadToEnd();
                OnStdOutWrite(output);
                process.WaitForExit();
                int exitCode = process.ExitCode;
                return exitCode == 0;
            }

            return false;
        }

        #endregion

        #region Stepping Requests

        protected override PauseResponse HandlePauseRequest(PauseArguments arguments)
        {
            _debugger.AbortStepping = true;
            _queueNotProcessingEvent.WaitOne();
            SendStoppedEvent(StoppedEvent.ReasonValue.Pause, (int)_debugger.CurrentThreadId);
            return new();
        }

        protected override NextResponse HandleNextRequest(NextArguments arguments)
        {
            _debugEvents.Add(new()
            {
                ThreadId = arguments.ThreadId,
                Type = EventType.Step,
            });

            return new();
        }

        protected override StepInResponse HandleStepInRequest(StepInArguments arguments)
        {
            _debugEvents.Add(new()
            {
                ThreadId = arguments.ThreadId,
                Type = EventType.StepIn,
            });

            return new();
        }

        protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments)
        {
            _debugEvents.Add(new()
            {
                ThreadId = arguments.ThreadId,
                Type = EventType.Continue,
            });
            return new();
        }

        #endregion

        protected override SetVariableResponse HandleSetVariableRequest(SetVariableArguments arguments)
        {
            if (_debugger.StateInformation is null)
            {
                _logger.LogError("Received scopes request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            if (!_debugger.StateInformation.Scopes.TryGetValue(arguments.VariablesReference, out ScopeState? scope))
            {
                _logger.LogError($"Cannot find scope with reference '{arguments.VariablesReference}'");
                SendTerminationEvent();
                return new();
            }

            IScopeNode? varToEdit = scope.Children.FirstOrDefault(v => v.Name.Equals(arguments.Name));
            if (varToEdit == null)
            {
                _logger.LogError($"Cannot find variable with name '{arguments.Name}' in scope '{arguments.VariablesReference}'");
                SendTerminationEvent();
                return new();
            }

            if (varToEdit.CanOverrideValue)
            {
                if (!varToEdit.OverrideValue(arguments.Value))
                {
                    _logger.LogError($"Cannot set variable with name '{arguments.Name}' in scope '{arguments.VariablesReference}' the value of '{arguments.Value}'");
                }
            }

            return new()
            {
                Value = varToEdit.Value?.ToString(),
                Type = varToEdit.ValueType.ToString(),
                VariablesReference = 0,
            };
        }

        protected override ThreadsResponse HandleThreadsRequest(ThreadsArguments arguments)
        {
            if (_debugger.StateInformation is null)
            {
                _logger.LogError("Received threads request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            List<Thread> dbgThreads = new();

            foreach (KeyValuePair<int, ThreadState> t in _debugger.StateInformation.Threads)
            {
                dbgThreads.Add(new Thread
                {
                    Id = t.Key,
                    Name = $"Thread_{t.Key}",
                });
            }

            return new ThreadsResponse { Threads = dbgThreads, };
        }

        protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments)
        {
            if (_debugger.StateInformation is null)
            {
                _logger.LogError("Received stack trace request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            if (!_debugger.StateInformation.Threads.TryGetValue(arguments.ThreadId, out ThreadState? state))
            {
                _logger.LogError($"Unkown thread '{arguments.ThreadId}', aborting!");
                SendTerminationEvent();
                return new();
            }

            int i = arguments.StartFrame ?? 0;
            int endCount = arguments.Levels ?? state.Frames.Count;
            if (endCount > state.Frames.Count)
            {
                endCount = state.Frames.Count;
            }

            List<StackFrame> frames = [];
            List<FrameState> dbgFrames = state.CallStack
                .Reverse()
                .ToList();

            for (; i < endCount; i++)
            {
                FrameState fState = dbgFrames[i];

                _dapSources.TryGetValue(fState.FunctionNamespace, out Source? value);

                StackFrame f = new(fState.FrameId, fState.FunctionName, fState.SourceLine + 1, 0)
                {
                    Source = value,
                };

                frames.Add(f);
            }

            return new()
            {
                StackFrames = frames,
                TotalFrames = state.Frames.Count,
            };
        }

        protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments)
        {
            if (_debugger.StateInformation is null)
            {
                _logger.LogError("Received scopes request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            FrameState? state = _debugger.StateInformation.GetFrameById(arguments.FrameId);
            if (state is null)
            {
                _logger.LogError($"Could not find frame with id '{arguments.FrameId}'!");
                SendTerminationEvent();
                return new();
            }

            List<Scope> scopes = new();
            foreach (KeyValuePair<int, ScopeState> kvp in state.Scopes.Where(s => s.Value.IsRootScope))
            {
                ScopeState dbgScope = kvp.Value;
                int varReference = dbgScope.Children.Count > 0 ? dbgScope.VarReference : 0;
                scopes.Add(new(dbgScope.Name, varReference, false));
            }

            return new()
            {
                Scopes = scopes,
            };
        }

        protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments)
        {
            if (_debugger.StateInformation is null)
            {
                _logger.LogError("Received variables request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            if (!_debugger.StateInformation.Scopes.TryGetValue(arguments.VariablesReference, out ScopeState? scope))
            {
                _logger.LogError("Received threads request but state was not populated!");
                SendTerminationEvent();
                return new();
            }

            List<Variable> variables = [];

            int i = arguments.Start ?? 0;
            int count = arguments.Count ?? scope.Children.Count;
            if (count < scope.Children.Count)
            {
                count = scope.Children.Count;
            }

            for (; i < count; i++)
            {
                IScopeNode dbgVar = scope.Children[i];
                int structuredId = dbgVar.Children.Count > 0 ? dbgVar.VarReference : 0;

                variables.Add(new(dbgVar.Name, dbgVar.Value?.ToString() ?? string.Empty, structuredId)
                {
                    Type = dbgVar.ValueType.ToString(),
                });
            }

            return new()
            {
                Variables = variables
            };
        }

        protected override TerminateResponse HandleTerminateRequest(TerminateArguments arguments)
        {
            _debugger.Dispose();
            TerminateTask();
            return new();
        }

        protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments)
        {
            _debugger.Dispose();
            return new();
        }

        private bool LoadNebulaScripts(IEnumerable<string> sourceFiles, LaunchArguments arguments, DebuggerConfiguration configuration)
        {
            string[] scriptFiles = GetScriptsToLoad(sourceFiles.ToArray());

            if (!_debugger.AddScripts(scriptFiles))
            {
                _logger.LogError($"Could not load one or more script, aborting debugger!");
                return false;
            }

            foreach (KeyValuePair<string, Commons.Debugger.DebugFile> loadedDbgFile in _debugger.DebugFiles)
            {
                _dapSources[loadedDbgFile.Value.Namespace] = new()
                {
                    Path = loadedDbgFile.Value.SourceFilePath,
                    Name = loadedDbgFile.Value.OriginalFileName,
                };
            }

            string nativeBindingSource = arguments.ConfigurationProperties.GetValueAsString(DebuggerConstants.BindingLookupPath);

            List<string> bindingFullPaths = [];
            if (!File.Exists(nativeBindingSource))
            {
                if (Directory.Exists(nativeBindingSource))
                {
                    bindingFullPaths.AddRange(Directory.GetFiles(nativeBindingSource, "*.dll"));
                }
            }
            else
            {
                bindingFullPaths.Add(nativeBindingSource);
            }

            HashSet<string> nativeFuncToLoad = _debugger.DebugFiles.Values.SelectMany(s => s.NativeFunctions)
                .ToHashSet();

            if (!_debugger.AddBindings(bindingFullPaths, nativeFuncToLoad))
            {
                _logger.LogError($"Could not load one or more binding file, aborting debugger!");
                return false;
            }

            return true;
        }

        private static IEnumerable<string> FetchAllSourceDirectories(LaunchArguments arguments)
        {
            string rootFolder = arguments.ConfigurationProperties.GetValueAsString(DebuggerConstants.Workspace);

            List<string> additionalFolders = [];
            additionalFolders.Add(rootFolder);

            JArray? jAddFolders = (JArray?)arguments.ConfigurationProperties.GetValueOrDefault(DebuggerConstants.AdditionalScriptFolders);
            if (jAddFolders is not null)
            {
                foreach (string? f in jAddFolders.Values<string>())
                {
                    if (!string.IsNullOrEmpty(f))
                    {
                        additionalFolders.Add(f);
                    }
                }
            }

            return additionalFolders;
        }

        private static string[] GetScriptsToLoad(IEnumerable<string> folders)
        {
            HashSet<string> files = [];
            foreach (string folder in folders)
            {
                foreach (string file in Directory.GetFiles(folder, "*.neb"))
                {
                    files.Add(file);
                }
            }

            //_logger.LogInformation($"Debugging with {files.Count} unique scripts");
            return files.ToArray();
        }

        private void OnStdOutWriteLine(string message)
        {
            Protocol.SendEvent(new OutputEvent(message + "\n")
            {
                Category = OutputEvent.CategoryValue.Stdout,
            });
        }

        private void OnStdOutWrite(string message)
        {
            Protocol.SendEvent(new OutputEvent(message)
            {
                Category = OutputEvent.CategoryValue.Stdout,
            });
        }
    }
}
