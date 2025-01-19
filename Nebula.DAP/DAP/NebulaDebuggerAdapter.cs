using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
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
        private readonly DebuggerConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly NebulaDebugger _nebulaDebugger;

        private Task? _dbgThread;
        private readonly CancellationTokenSource _tokSource = new();
        private readonly ManualResetEventSlim _runEvent = new(true);
        private readonly object _lockSync = new();
        private StoppedEvent? _stopEvent;

        internal NebulaDebuggerAdapter(DebuggerConfiguration debuggerConfiguration, ILogger logger)
        {
            _configuration = debuggerConfiguration;
            _logger = logger;
            _nebulaDebugger = new NebulaDebugger(debuggerConfiguration, logger);

            InitializeProtocolClient(debuggerConfiguration.InStream, debuggerConfiguration.Outstream);
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

        private void TaskDbgThread()
        {
            _nebulaDebugger.Init(_configuration.StepOnEntry);
            while (!_tokSource.IsCancellationRequested)
            {
                lock (_lockSync)
                {
                    // If event was cancelled
                    if (!_runEvent.Wait(0))
                    {
                        if (_stopEvent == null)
                            throw new InvalidOperationException("Stop reason is not set");

                        _nebulaDebugger.ReloadStateInformation();
                        Protocol.SendEvent(_stopEvent);
                        _stopEvent = null;
                    }
                }

                _runEvent.Wait();

                while (_runEvent.IsSet)
                {
                    bool isSingleStep = _stopEvent != null && _stopEvent.Reason == StoppedEvent.ReasonValue.Step;
                    int threadId = -1;

                    if (isSingleStep)
                    {
                        threadId = _stopEvent!.ThreadId.GetValueOrDefault();
                        _runEvent.Reset();
                    }

                    _nebulaDebugger.Step(threadId);
                    _nebulaDebugger.ReloadStateInformation();
                }
            }

            Protocol.SendEvent(new ExitedEvent(exitCode: 0));
            Protocol.SendEvent(new TerminatedEvent());
        }

        private void PostStopReason(StoppedEvent.ReasonValue reason, int threadId)
        {
            lock (_lockSync)
            {
                _stopEvent = new()
                {
                    Reason = reason,
                    ThreadId = 0,
                    AllThreadsStopped = true
                };
                _runEvent.Reset();
            }
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
                SupportsFunctionBreakpoints = false,
                SupportsExceptionFilterOptions = false,
                SupportsDebuggerProperties = true,
                SupportsConfigurationDoneRequest = true,
            };
        }
        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            return new();
        }

        #endregion

        #region BreakpointsRequests

        protected override SetFunctionBreakpointsResponse HandleSetFunctionBreakpointsRequest(SetFunctionBreakpointsArguments arguments)
        {
            return new()
            {
                Breakpoints = []
            };
        }

        protected override SetExceptionBreakpointsResponse HandleSetExceptionBreakpointsRequest(SetExceptionBreakpointsArguments arguments)
        {
            return new()
            {
                Breakpoints = []
            };
        }

        #endregion

        protected override LaunchResponse HandleLaunchRequest(LaunchArguments arguments)
        {
            string rootFolder = arguments.ConfigurationProperties.GetValueAsString("workspace");
            List<string> additionalFolders = [];
            additionalFolders.Add(rootFolder);
            JArray? jAddFolders = (JArray?)arguments.ConfigurationProperties.GetValueOrDefault("add_deps");
            if (jAddFolders is not null)
            {
                foreach (string? f in jAddFolders.Values<string>())
                    additionalFolders.Add(f);
            }

            string[] scriptFiles = GetScriptsToLoad(additionalFolders.ToArray());
            _nebulaDebugger.LoadScripts(scriptFiles);

            _dbgThread = Task.Run(TaskDbgThread, _tokSource.Token);

            if (_configuration.StepOnEntry)
            {
                PostStopReason(StoppedEvent.ReasonValue.Entry, 0);
            }

            return new LaunchResponse();
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

        protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments)
        {
            _nebulaDebugger.StopAndReset();
            return new DisconnectResponse();
        }

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
            return new()
            {
                AllThreadsContinued = false,
            };
        }

        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            if (arguments.Breakpoints == null)
                throw new ProtocolException("No breakpoints set");

            //List<Breakpoint> responseBreakpoints;
            //foreach (var sourceBreakpoint in arguments.Breakpoints)
            //{
            //    if (_nebulaDebugger.Breakpoints.IsLineNumberDebuggable(arguments.Source.Name, sourceBreakpoint.Line, out int resolvedLine))
            //    {
            //
            //    }
            //}

            return new SetBreakpointsResponse();
        }

        protected override ThreadsResponse HandleThreadsRequest(ThreadsArguments arguments)
        {
            List<Thread> vmThreads = [];
            foreach (NebulaThread nt in _nebulaDebugger.Threads.Values)
                vmThreads.Add(new Thread
                {
                    Id = nt.ThreadId,
                    Name = nt.ThreadName
                });

            return new ThreadsResponse()
            {
                Threads = vmThreads,
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
                return new();

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
                return response;

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
    }
}
