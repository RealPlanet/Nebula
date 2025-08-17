using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
using Nebula.Commons.Debugger;
using Nebula.Debugger.Bridge;
using Nebula.Debugger.Bridge.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thread = Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Thread;

namespace Nebula.Debugger.DAP
{
    public sealed class NebulaDebuggerAdapter
        : DebugAdapterBase
    {
        private enum ExitReason
        {
            NormalExit,
            InitError,
        }

        private readonly DebuggerConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly NebulaDebugger _nebulaDebugger;

        private Task? _dbgThread;
        private readonly CancellationTokenSource _tokSource = new();
        private readonly ManualResetEventSlim _runEvent = new(true);
        private readonly object _lockSync = new();
        private StoppedEvent? _stopEvent;
        private bool _isStepIn;

        internal NebulaDebuggerAdapter(DebuggerConfiguration debuggerConfiguration, ILogger logger)
        {
            _configuration = debuggerConfiguration;
            _logger = logger;
            _nebulaDebugger = new NebulaDebugger(debuggerConfiguration, logger);
            _nebulaDebugger.ExecutionEnd += OnExecutionEnd;
            _nebulaDebugger.OnStdOut += OnStdOutFromNative;
            InitializeProtocolClient(debuggerConfiguration.InStream, debuggerConfiguration.Outstream);
        }

        private void OnStdOutFromNative(object? sender, string e)
        {
            Protocol.SendEvent(new OutputEvent(e)
            {
                Category = OutputEvent.CategoryValue.Stdout
            });
        }

        private void OnExecutionEnd(object? sender, EventArgs e)
        {
            _tokSource.Cancel();
            _runEvent.Reset();
            _nebulaDebugger.Dispose();
        }

        #region Private

        private void Continue(bool step, int threadId)
        {
            lock (_lockSync)
            {
                if (step)
                {
                    PostStopReason(StoppedEvent.ReasonValue.Step, threadId);
                }
            }

            _runEvent.Set();
        }

        private async void TaskDbgThread()
        {
            // Cheap trick to ensure breakpoints arrive on time before we start running the vm
            // This is to avoid missing breakpoints on the first lines
            await Task.Delay(100);
            _nebulaDebugger.Init();
            while (!_tokSource.IsCancellationRequested)
            {
                lock (_lockSync)
                {
                    // If event was cancelled
                    if (!_runEvent.Wait(0))
                    {
                        if (_stopEvent == null)
                        {
                            throw new InvalidOperationException("Stop reason is not set");
                        }

                        _nebulaDebugger.ReloadStateInformation();
                        Protocol.SendEvent(_stopEvent);
                        _stopEvent = null;
                    }
                }

                _runEvent.Wait();

                while (_runEvent.IsSet)
                {
                    bool isSingleStep = _stopEvent != null && _stopEvent.Reason == StoppedEvent.ReasonValue.Step;
                    bool isStepIn = _isStepIn;
                    int threadId = -1;

                    if (isSingleStep)
                    {
                        _logger.LogInformation($"Stepping thread {_stopEvent!.ThreadId}");
                        threadId = _stopEvent!.ThreadId.GetValueOrDefault();
                        _isStepIn = false;
                        _runEvent.Reset();
                    }

                    _nebulaDebugger.Step(threadId, isStepIn, out int stoppedThreadId, out StoppedEvent.ReasonValue? stopReason);
                    if (stopReason != null)
                    {
                        StoppedEvent? newStopEvent = _stopEvent;
                        newStopEvent ??= new();

                        newStopEvent.ThreadId = stoppedThreadId;
                        // TODO :: Figure out reason
                        newStopEvent.Reason = (StoppedEvent.ReasonValue)stopReason;
                        newStopEvent.AllThreadsStopped = true;
                        PostStopReason(newStopEvent);
                    }

                }
            }

            PostTerminationEvents();
        }

        private void PostTerminationEvents(ExitReason reason = ExitReason.NormalExit)
        {
            StringWriter message = new();
            OutputEvent.CategoryValue messageType = OutputEvent.CategoryValue.Console;

            message.WriteLine(">> Terminating debugger <<");

            switch (reason)
            {
                case ExitReason.InitError:
                    message.WriteLine(" One or more initialization errors stopped the debugger!");
                    break;
            }

            OutputEvent outputEvent = new OutputEvent()
            {
                Output = message.ToString(),
                Category = messageType,
            };
            Protocol.SendEvent(outputEvent);
            Protocol.SendEvent(new ExitedEvent(exitCode: (int)reason));
            Protocol.SendEvent(new TerminatedEvent());
        }

        private void PostStopReason(StoppedEvent newStopEvent)
        {
            lock (_lockSync)
            {
                _stopEvent = newStopEvent;
                _runEvent.Reset();
            }
        }

        private void PostStopReason(StoppedEvent.ReasonValue reason, int threadId)
        {
            lock (_lockSync)
            {
                _stopEvent = new()
                {
                    Reason = reason,
                    ThreadId = threadId,
                    AllThreadsStopped = true
                };
                _runEvent.Reset();
            }
        }

        private void LoadDebuggerScripts(LaunchArguments arguments)
        {
            string rootFolder = arguments.ConfigurationProperties.GetValueAsString("workspace");
            string nativeBindingSource = arguments.ConfigurationProperties.GetValueAsString("native_binding_dll");

            List<string> additionalFolders = [];
            additionalFolders.Add(rootFolder);
            JArray? jAddFolders = (JArray?)arguments.ConfigurationProperties.GetValueOrDefault("add_deps");
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

            string[] scriptFiles = GetScriptsToLoad(additionalFolders.ToArray());
            if (!_nebulaDebugger.InitDebugger(scriptFiles, nativeBindingSource))
            {
                OnExecutionEnd(this, EventArgs.Empty);
            }
        }

        private string[] GetScriptsToLoad(string[] folders)
        {
            HashSet<string> files = new();
            foreach (string folder in folders)
            {
                foreach (string file in Directory.GetFiles(folder, "*.neb"))
                {
                    files.Add(file);
                }
            }

            _logger.LogInformation($"Debugging with {files.Count} unique scripts");
            return files.ToArray();
        }
        #endregion

        #region API
        /// <summary>
        /// <inheritdoc cref="DebugProtocolClient.Run"/>
        /// </summary>
        internal void Run()
        {
            Protocol.Run();
        }

        #endregion

        #region DAP Callbacks

        #region Initialization
        protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments)
        {
            Protocol.SendEvent(new InitializedEvent());
            return new InitializeResponse()
            {
                SupportsSingleThreadExecutionRequests = false, // All or nothing
                SupportsConfigurationDoneRequest = true,
                SupportsSetVariable = true,
                SupportsFunctionBreakpoints = true,
                SupportsDebuggerProperties = true,
                SupportsInstructionBreakpoints = false,
                SupportsExceptionFilterOptions = false,
            };
        }
        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            return new();
        }

        #endregion

        #region Breakpoint Requests

        protected override SetFunctionBreakpointsResponse HandleSetFunctionBreakpointsRequest(SetFunctionBreakpointsArguments arguments)
        {
            List<Breakpoint> actualBreakpoints = [];
            _nebulaDebugger.Breakpoints.ClearFunctionBreakpoints();
            foreach (FunctionBreakpoint? funcBreakpoint in arguments.Breakpoints)
            {
                string funcName = funcBreakpoint.Name;
                Breakpoint bp = new()
                {
                    Verified = false,
                    Line = -1,
                    Source = null,
                    Reason = Breakpoint.ReasonValue.Failed
                };

                actualBreakpoints.Add(bp);

                if (funcName.Contains("::"))
                {
                    // Namespace :: Funcname
                    string[] tokens = funcName.Split("::", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (tokens.Length != 2)
                    {
                        _logger.LogWarning($"Invalid function breakpoint '{funcName}'");
                        bp.Message = "Could not split into namespace and function name (expected one '::' token as separator)";

                        continue;
                    }
                    string ns = tokens[0];
                    funcName = tokens[1];

                    if (!_nebulaDebugger.DebugFiles.TryGetValue(ns, out DebugFile? dbgFile))
                    {
                        _logger.LogWarning($"Namespace for function breakpoint '{ns}' not found in dbg files");
                        bp.Message = "Could not find namespace";

                        continue;
                    }

                    if (!dbgFile.Functions.TryGetValue(funcName, out DebugFunction? dbgFunc))
                    {
                        _logger.LogWarning($"Function '{funcName}' in namespace '{ns}' not found in dbg files for function breakpoint");
                        bp.Message = "Could not find function in namespace";
                        continue;
                    }

                    bp.Verified = true;
                    bp.Line = dbgFunc.LineNumber;
                    bp.Source = _nebulaDebugger.DAPSources[ns];
                    bp.Reason = null;

                    _nebulaDebugger.Breakpoints.AddFunctionBreakpoint(new(ns, funcName, dbgFunc.LineNumber));
                }
                else
                {
                    //TODO :: Break on all functions in all namespaces with the same name
                    throw new ProtocolException("Invalid function breakpoint");
                }
            }
            return new(actualBreakpoints);
        }

        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            List<Breakpoint> actualBreakpoints = [];
            string @namespace = string.Empty;
            Source? inMemorySource = null;
            foreach (KeyValuePair<string, Source> kvp in _nebulaDebugger.DAPSources)
            {
                if (kvp.Value.Name == arguments.Source.Name)
                {
                    @namespace = kvp.Key;
                    inMemorySource = kvp.Value;
                }
            }

            _nebulaDebugger.Breakpoints.ClearGenericBreakpoints(@namespace);
            foreach (SourceBreakpoint? reqBp in arguments.Breakpoints)
            {
                Breakpoint bp = new()
                {
                    Verified = false,
                    Line = -1,
                    Source = null,
                    Reason = Breakpoint.ReasonValue.Failed,
                };
                actualBreakpoints.Add(bp);

                if (inMemorySource is null)
                {
                    bp.Message = "Source could not be found in debug files";
                    continue;
                }


                int targetLine = reqBp.Line;
                bool isDbg = _nebulaDebugger.IsLineDebuggable(@namespace, targetLine, out string functionName, out int instructionIndex);
                if (!isDbg)
                {
                    bp.Message = "Line is not debuggable";
                    continue;
                }


                bp.Line = targetLine;
                bp.Verified = true;

                _nebulaDebugger.Breakpoints.AddBreakpoint(new NebulaBreakpoint(@namespace, functionName, instructionIndex));
            }

            //_loadingBreakpointsEvent.Reset();
            return new(actualBreakpoints);
        }
        #endregion

        #region Control Operations

        protected override NextResponse HandleNextRequest(NextArguments arguments)
        {
            Continue(true, arguments.ThreadId);
            return new NextResponse();
        }

        protected override PauseResponse HandlePauseRequest(PauseArguments arguments)
        {
            PostStopReason(StoppedEvent.ReasonValue.Pause, arguments.ThreadId);
            return new();
        }

        protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments)
        {
            Continue(false, arguments.ThreadId);
            return new()
            {
                AllThreadsContinued = true,
            };
        }

        protected override StepInResponse HandleStepInRequest(StepInArguments arguments)
        {
            _isStepIn = true;
            Continue(true, arguments.ThreadId);
            return new StepInResponse();
        }

        #endregion

        #region Base operations
        protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments)
        {
            bool debugInternals = arguments.ConfigurationProperties.GetValueAsBool("debug_dap") ?? false;
            if (debugInternals)
            {
                System.Diagnostics.Debugger.Launch();
            }

            bool stopOnEntry = arguments.ConfigurationProperties.GetValueAsBool("stopOnEntry") ?? false;
            _configuration.StepOnEntry = stopOnEntry;

            LoadDebuggerScripts(arguments);

            _dbgThread = Task.Run(TaskDbgThread, _tokSource.Token);


            if (_dbgThread.IsCanceled)
            {
                PostTerminationEvents(ExitReason.InitError);
            }

            if (_configuration.StepOnEntry)
            {
                PostStopReason(StoppedEvent.ReasonValue.Entry, 0);
            }

            return new LaunchResponse();
        }

        protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments)
        {
            _nebulaDebugger.StopAndReset();
            return new DisconnectResponse();
        }

        protected override ThreadsResponse HandleThreadsRequest(ThreadsArguments arguments)
        {
            List<Thread> vmThreads = [];
            foreach (NebulaThread nt in _nebulaDebugger.Threads.Values)
            {
                vmThreads.Add(new Thread
                {
                    Id = nt.ThreadId,
                    Name = nt.ThreadName
                });
            }

            return new ThreadsResponse()
            {
                Threads = vmThreads
            };
        }

        protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments)
        {
            int threadId = arguments.ThreadId;

            NebulaThread thread = _nebulaDebugger.Threads[threadId];
            IList<NebulaStackFrame> stack = thread.StackTrace;

            List<StackFrame> msFrames = new();
            foreach (NebulaStackFrame f in stack)
            {
                int currentLineNumber = _nebulaDebugger.GetLineNumber(f);
                msFrames.Add(new StackFrame
                {
                    Id = f.FrameId,
                    Name = f.NativeFrame.FunctionName,
                    Line = currentLineNumber + 1,
                    Source = _nebulaDebugger.DAPSources[f.NativeFrame.Namespace]
                });
            }

            return new()
            {
                TotalFrames = msFrames.Count,
                StackFrames = msFrames
            };
        }

        protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments)
        {
            int frameId = arguments.FrameId;
            // TODO :: To optimize ?
            NebulaStackFrame? myFrame = _nebulaDebugger.Threads
                .SelectMany(t => t.Value.StackTrace)
                .FirstOrDefault(f => f.FrameId == frameId);

            if (myFrame is null)
            {
                return new();
            }

            ScopesResponse response = new();
            foreach (NebulaScope nScope in myFrame.Scopes)
            {
                int varReference = nScope.Variables.Count > 0 ? nScope.ScopeId : 0;
                response.Scopes.Add(new(nScope.Name, nScope.ScopeId, false));
            }

            // Scope s;
            return response;
        }

        protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments)
        {
            NebulaScope? myScope = _nebulaDebugger.Threads
                .SelectMany(t => t.Value.StackTrace)
                .SelectMany(f => f.Scopes)
                .FirstOrDefault(s => s.ScopeId == arguments.VariablesReference);

            VariablesResponse response = new();
            if (myScope is null)
            {
                return response;
            }

            response.Variables.Capacity = myScope.Variables.Capacity;
            foreach (NebulaVariable v in myScope.Variables)
            {
                response.Variables.Add(new Variable(v.Name, v.Value, 0)
                {
                    Type = v.Type
                });
            }

            return response;
        }
        #endregion

        #region Expression Operations

        protected override SetVariableResponse HandleSetVariableRequest(SetVariableArguments arguments)
        {
            NebulaScope? myScope = _nebulaDebugger.Threads
                .SelectMany(t => t.Value.StackTrace)
                .SelectMany(f => f.Scopes)
                .FirstOrDefault(s => s.ScopeId == arguments.VariablesReference);

            NebulaVariable myVar = myScope!.Variables.First(v => v.Name == arguments.Name);
            myVar.NativeVariable.Set(arguments.Value);

            return new()
            {
                VariablesReference = arguments.VariablesReference,
                Type = myVar.Type,
                Value = myVar.Value
            };
        }

        #endregion

        #endregion
    }
}
