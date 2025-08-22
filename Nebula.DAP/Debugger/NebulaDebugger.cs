using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Nebula.Commons.Debugger;
using Nebula.Commons.Text;
using Nebula.Debugger.Debugger.Data;
using Nebula.Interop.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nebula.Debugger.Debugger
{
    public delegate void EmptyEventHandler();

    public sealed class NebulaDebugger
        : IDisposable
    {
        public EmptyEventHandler? DebugFilesChanged;
        public BreakpointManager BreakpointManager => _breakpointManager;
        public IReadOnlyDictionary<string, DebugFile> DebugFiles => _debugFiles;
        public long CurrentThreadId => _nativeVm.CurrentThreadId;
        public VirtualMachineState? StateInformation { get; private set; }
        public bool AbortStepping { get; set; } = false;

        private readonly VirtualMachine _nativeVm = new();
        private readonly ILogger _logger;
        private readonly BreakpointManager _breakpointManager;

        private readonly Dictionary<string, DebugFile> _debugFiles = [];

        public NebulaDebugger(ILogger logger)
        {
            _logger = logger;
            _breakpointManager = new(logger);
        }

        public void RedirectOutput(StdOutEventHandler writeCb, StdOutEventHandler writeLineCn)
        {
            _nativeVm.RedirectStdOutput(writeCb, writeLineCn);
        }

        public void StepInstruction()
        {
            _nativeVm.Step();
        }

        public bool StepLine(int threadId, out HitBreakpointInformation? hitBreakpoint)
        {
            hitBreakpoint = null;
            if (StateInformation is null)
            {
                return false;
            }

            ThreadState thread = StateInformation.Threads[threadId];

            // Callstack gets updated real time
            Interop.Structures.Callstack callstack = thread.OriginalCallstack;

            Interop.Structures.Frame lastFrame = callstack.LastFrame;
            int callstackCount = thread.OriginalCallstack.FrameCount;
            int currentOpcode = lastFrame.NextInstructionIndex;

            if (!_debugFiles.TryGetValue(lastFrame.Namespace, out DebugFile? dbgFile) ||
                !dbgFile.Functions.TryGetValue(lastFrame.FunctionName, out DebugFunction? dbgFunc))
            {
                StepInstruction();
                return true;
            }

            int line = VirtualMachineState.GetLineNumber(dbgFunc, currentOpcode);
            int nextLine = line;
            if (SteppingOverLastInstruction(threadId, currentOpcode, dbgFunc))
            {
                return true;
            }

            do
            {
                StepInstruction();

                currentOpcode = lastFrame.NextInstructionIndex;
                if (SteppingOverLastInstruction(threadId, currentOpcode, dbgFunc))
                {
                    return true;
                }

                Interop.Structures.Frame newLastFrame = callstack.LastFrame;
                while (newLastFrame.FunctionName != lastFrame.FunctionName ||
                    newLastFrame.Namespace != lastFrame.Namespace ||
                    thread.OriginalCallstack.FrameCount != callstackCount)
                {
                    if (AbortStepping)
                    {
                        return true;
                    }

                    // We called a function, need to keep stepping until we get back here
                    StepInstruction();
                    newLastFrame = callstack.LastFrame; // Get again the last frame
                }

                lastFrame = newLastFrame;
                nextLine = VirtualMachineState.GetLineNumber(dbgFunc, lastFrame.NextInstructionIndex);
            } while (line == nextLine && !AbortStepping);

            return true;
        }

        public bool StepStatement(int threadId, out HitBreakpointInformation? hitBreakpoint)
        {
            return StepLine(threadId, out hitBreakpoint);
        }

        public bool StepIn(int threadId, out HitBreakpointInformation? hitBreakpoint)
        {
            hitBreakpoint = null;
            if (StateInformation is null)
            {
                return false;
            }

            ThreadState thread = StateInformation.Threads[threadId];
            Interop.Structures.Frame lastFrame = thread.OriginalCallstack.LastFrame;
            var currentOpcode = lastFrame.NextInstructionIndex;
            if (!_debugFiles.TryGetValue(lastFrame.Namespace, out DebugFile? dbgFile) ||
                !dbgFile.Functions.TryGetValue(lastFrame.FunctionName, out DebugFunction? dbgFunc))
            {
                StepInstruction();
                return true;
            }

            int line = VirtualMachineState.GetLineNumber(dbgFunc, currentOpcode);
            int nextLine = line;
            do
            {
                StepInstruction();
                Interop.Structures.Frame newLastFrame = thread.OriginalCallstack.LastFrame;

                if (lastFrame.FunctionName != newLastFrame.FunctionName || lastFrame.Namespace != lastFrame.Namespace)
                {
                    break;
                }

                lastFrame = newLastFrame;
                nextLine = VirtualMachineState.GetLineNumber(dbgFunc, newLastFrame.NextInstructionIndex);
            } while (line == nextLine);

            return true;
        }

        public bool Continue(out HitBreakpointInformation? hitBreakpoint)
        {
            hitBreakpoint = null;
            while (!AbortStepping)
            {
                StepInstruction();
                hitBreakpoint = AnyBreakpointsHit();
                if (hitBreakpoint != null)
                {
                    break;
                }
            }

            return true;
        }

        public bool AddScripts(string[] scriptFiles)
        {
            if (!_nativeVm.AddScripts(scriptFiles, OnScriptParseError))
            {
                _logger.LogWarning($"Could not add scripts to virtual machine!");
                return false;
            }

            bool addedDbgFiles = false;
            foreach (string script in scriptFiles)
            {
                try
                {
                    string dbgPath = Path.ChangeExtension(script, ".ndbg");
                    if (!File.Exists(dbgPath))
                    {
                        _logger.LogWarning($"Could not find debug data for script '{script}'");
                        continue;
                    }

                    addedDbgFiles = true;
                    SourceCode source = SourceCode.From(script);
                    DebugFile file = DebugFile.LoadFromFile(dbgPath);

                    // TODO :: Source files might be somewhere else?
                    string sourceFilePath = Path.GetDirectoryName(dbgPath)!;
                    sourceFilePath = Path.Combine(sourceFilePath, file.OriginalFileName);
                    file.SourceFilePath = sourceFilePath;

                    _debugFiles.Add(file.Namespace, file);
                    _logger.LogInformation($"Loaded source and debug info of script '{script}'");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while loading debug information: {e}");
                }
            }

            if (addedDbgFiles)
            {
                DebugFilesChanged?.Invoke();
            }

            return true;
        }

        public bool AddBindings(ICollection<string> bindingsLocation, ISet<string>? nativeFuncToLoad)
        {
            if (nativeFuncToLoad is null || nativeFuncToLoad.Count == 0)
            {
                foreach (string path in bindingsLocation)
                {
                    if (!_nativeVm.LoadNativesFromDll(path))
                    {
                        _logger.LogError($"Could not load bindings from library '{path}'");
                        return false;
                    }
                }

                return true;
            }

            foreach (string path in bindingsLocation)
            {
                if (!_nativeVm.LoadNativesFromDll(path, nativeFuncToLoad))
                {
                    _logger.LogError($"Could not load bindings from library '{path}'");
                    return false;
                }
            }

            return true;
        }

        private void OnScriptParseError(string path, ReportType type, string message)
        {
            _logger.LogInformation($"[{path}] [{type}] {message}");
        }

        public void Initialize(bool startPause)
        {
            _nativeVm.Initialize(startPause);
        }

        public DebugFunction? GetDebugInfo(string @namespace, string functionName)
        {
            if (!DebugFiles.TryGetValue(@namespace, out DebugFile? file))
            {
                return null;
            }

            if (!file.Functions.TryGetValue(functionName, out DebugFunction? func))
            {
                return null;
            }

            return func;
        }

        public void ReloadVirtualMachineState()
        {
            if (_nativeVm.VMState != VirtualMachine.State.Paused)
            {
                _logger.LogError($"Cannot refresh state while virtual machine is running!");
                throw new NotSupportedException($"Cannot refresh state while virtual machine is running!");
            }

            StateInformation = new VirtualMachineState(this);

            int threadCount = _nativeVm.ThreadCount;
            for (int i = 0; i < threadCount; i++)
            {
                Interop.Structures.Callstack callstack = _nativeVm.GetCallstackOfThread(i);
                if (callstack == null)
                {
                    _logger.LogWarning($"Callstack of thread '{i}' is null!");
                    continue;
                }

                ThreadState tState = new(StateInformation, i, callstack);
                foreach (var frame in callstack.Frames)
                {
                    int frameId = StateInformation.GetNextFrameId();
                    FrameState fState = new(StateInformation, tState, frameId, frame);
                    tState.AddFrame(fState);
                }

                StateInformation.AddThread(tState);
            }
        }

        /// <summary> Given a namespace and a line number this method checks if the line is within a function and if so returns the function name and instruction index</summary>
        public bool IsLineDebuggable(string @namespace, int line, out string functionName, out int instructionIndex)
        {
            line--; // Debug files are 0 based
            functionName = string.Empty;
            instructionIndex = 0;

            if (!_debugFiles.TryGetValue(@namespace, out DebugFile? file))
            {
                return false;
            }

            DebugFunction? targetFunc = file.Functions.Values.FirstOrDefault(f => line >= f.LineNumber && line < f.EndLineNumber);

            if (targetFunc is null)
            {
                return false;
            }

            functionName = targetFunc.Name;

            // Normalize in range of function

            int lastOpcode = -1;
            int count = 0;
            foreach (KeyValuePair<int, int> d in targetFunc.LineStartingOpcodeIndex)
            {
                if (d.Key == line)
                {
                    instructionIndex = d.Value;
                    return true;
                }

                count++;
                if (d.Key <= line)
                {
                    lastOpcode = d.Value;
                    continue;
                }

                instructionIndex = lastOpcode;
                break;
            }

            return true;
        }

        public void Dispose()
        {
            _nativeVm.Dispose();
        }

        private bool SteppingOverLastInstruction(int threadId, int currentOpcode, DebugFunction dbgFunc)
        {
            if (currentOpcode == dbgFunc.InstructionCount - 1)
            {
                // Keep stepping until we reach our thread
                while (_nativeVm.CurrentThreadId != threadId)
                {
                    StepInstruction();
                }

                // We step once, this will cause the function to return and the callstack to be deleted if it's empty
                StepInstruction();
                return true;
            }

            return false;
        }

        private HitBreakpointInformation? AnyBreakpointsHit()
        {
            foreach (BreakpointInformation fbp in _breakpointManager.FunctionBreakpoints)
            {
                int threadId = _nativeVm.AnyFrameJustStarted(fbp.Namespace, fbp.FunctionName);
                if (threadId >= 0)
                {
                    return new()
                    {
                        ThreadId = threadId,
                        IsFunctionBreakpoint = true,
                    };
                }
            }

            foreach (var kvp in _breakpointManager.Breakpoints)
            {
                var breakPoints = kvp.Value;
                string @namespace = kvp.Key;

                foreach (var breakpoint in breakPoints)
                {
                    int threadId = _nativeVm.AnyFrameAboutToBeAt(@namespace,
                                                                breakpoint.FunctionName,
                                                                breakpoint.OpcodeIndex);

                    if (threadId >= 0)
                    {
                        return new()
                        {
                            ThreadId = threadId,
                            IsFunctionBreakpoint = false
                        };
                    }
                }
            }

            return null;
        }
    }
}
