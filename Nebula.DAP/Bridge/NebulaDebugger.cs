﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Nebula.Commons.Text;
using Nebula.Debugger.Bridge.Objects;
using Nebula.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Nebula.Debugger.Bridge
{

    public class NebulaDebugger
        : IDisposable
    {
        #region Events

        public event EventHandler? ExecutionEnd;
        public event EventHandler? DebugFilesChanged;
        #endregion

        #region Properties

        internal IDictionary<int, NebulaThread> Threads => _latestStateInformation.Threads;
        internal DebuggerConfiguration Configuration { get; }
        internal IReadOnlyDictionary<string, DebugFile> DebugFiles => _dbgFiles;
        internal BreakpointManager Breakpoints { get; }
        internal InterpreterState VmState => _interpreter.State;
        internal Dictionary<string, Source> DAPSources { get; } = new();

        #endregion

        #region Fields
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly InterpreterW _interpreter;
        private readonly ILogger _logger;
        private readonly Dictionary<string, DebugFile> _dbgFiles = [];
        private StateInformation _latestStateInformation;
        #endregion

        public NebulaDebugger(DebuggerConfiguration configuration, ILogger logger)
        {
            InteropLogger loggerBridge = new(logger);
            Configuration = configuration;
            _logger = logger;
            _interpreter = new(loggerBridge);
            _latestStateInformation = new(this);
            Breakpoints = new BreakpointManager(this);

            _logger.LogInformation("Debugger constructed");

            //DebugFilesChanged += ReloadLineNumberCache;
        }

        //private void ReloadLineNumberCache(object? sender, EventArgs e)
        //{
        //    _debuggableLines.Clear();
        //    var lines = _dbgFiles.Values.SelectMany(s => s.Functions.Values)
        //        .SelectMany(v => v.InstructionLines);
        //    foreach (var l in lines)
        //        _debuggableLines.Add(l);
        //}

        #region Initialization

        public void Init()
        {
            _interpreter.Init(true);
        }

        public void StopAndReset()
        {
            _interpreter.Stop();
            _interpreter.Reset();
            _latestStateInformation = new(this);
        }

        /// <summary>
        /// TODO :: Rewrite this into something more managable, it has grown to have a different logic between stepping and continuing without a thread id.
        /// </summary>
        public bool Step(int threadId, bool isStepIn, out int stoppedThreadId, out StoppedEvent.ReasonValue? stopReason)
        {
            stoppedThreadId = threadId;
            stopReason = null;
            // Continue step
            if (threadId < 0)
            {
                int[] startingOpcodes = _interpreter.GetCurrentOpcodeIndexForAllThreads();

                _interpreter.Step();


                if (ShouldEarlyExitFromStepping(out int earlyStoppedThread, out stopReason))
                {
                    // Only consider early exits when we move at least one line for the stopping thread, otherwise we'll keep breaking on the same breakpoint
                    CallStackW earlyStoppingStack = _interpreter.GetStackFrameOf(earlyStoppedThread);
                    StackFrameW frame = earlyStoppingStack.LastFrame;
                    DebugFile currentDbgInfo = DebugFiles[frame.Namespace];
                    DebugFunction currentDbgFunction = currentDbgInfo.Functions[frame.FunctionName];
                    int currentOpcodeIndex = frame.CurrentInstructionIndex;


                    int startingLine = currentDbgFunction.LineNumber;
                    if (startingOpcodes[earlyStoppedThread] >= 0)
                        startingLine = currentDbgFunction.InstructionLines[startingOpcodes[earlyStoppedThread]];
                    int steppedLine = currentDbgFunction.LineNumber;
                    if (currentOpcodeIndex >= 0)
                        steppedLine = currentDbgFunction.InstructionLines[currentOpcodeIndex];

                    if (startingLine != steppedLine)
                    {
                        // Stop on a specific thread
                        stoppedThreadId = earlyStoppedThread;
                    }
                    else
                    {
                        stopReason = null;
                    }

                }

                if (_interpreter.State == InterpreterState.Exited)
                    ExecutionEnd?.Invoke(this, EventArgs.Empty);

                return true;
            }

            // Step is meant to skip one source line but native step means one opcode, we need to figure out how many opcodes to skip
            CallStackW stack = _interpreter.GetStackFrameOf(threadId);
            if (stack != null)
            {
                int stackCount = stack.FrameCount;
                StackFrameW frame = stack.LastFrame;
                DebugFile currentDbgInfo = DebugFiles[frame.Namespace];
                DebugFunction currentDbgFunction = currentDbgInfo.Functions[frame.FunctionName];
                int currentOpcode = frame.CurrentInstructionIndex;

                int numberOfSteps = 0;
                int lineNumber = currentDbgFunction.InstructionLines[currentOpcode];
                if (currentOpcode < currentDbgFunction.InstructionLines.Count)
                {
                    while (currentOpcode < frame.InstructionCount &&
                        currentDbgFunction.InstructionLines[currentOpcode + numberOfSteps] == lineNumber)
                    {
                        numberOfSteps++;
                    }
                }

                int targetOpcode = frame.CurrentInstructionIndex + numberOfSteps;
                // If we want to step in we'll never reach the target opcode because w'd have to step over the function call
                if (isStepIn)
                {
                    targetOpcode--;
                }

                bool exitLoop = false;
                while (!exitLoop &&
                    frame.CurrentInstructionIndex != targetOpcode &&
                    frame.CurrentInstructionIndex < frame.InstructionCount) // Happens when we do the last opcode
                {
                    // This condition triggers when we are about to execute a return statement in a function and thus de-allocating the frame
                    exitLoop = frame.CurrentInstructionIndex + 1 == targetOpcode &&
                        targetOpcode == frame.InstructionCount - 1;

                    _interpreter.Step();
                    CallStackW latestStack = _interpreter.GetStackFrameOf(threadId);
                    StackFrameW? lastFrame = latestStack?.LastFrame;
                    if (latestStack is null || lastFrame is null ||
                        latestStack.LastFrame.Namespace != currentDbgInfo.Namespace || latestStack.LastFrame.FunctionName != currentDbgFunction.Name)
                    {
                        // We exited the function
                        if (ShouldEarlyExitFromStepping(out int earlyStoppedThread, out stopReason))
                        {
                            // Stop on a specific thread
                            stoppedThreadId = earlyStoppedThread;
                            break;
                        }
                    }
                    else
                    {
                        int currentLineNumber = currentDbgFunction.InstructionLines[frame.CurrentInstructionIndex];
                        // This is to prevent breaking on the same line because of a breakpoint
                        // If we started from this line then we likely broke here
                        if (lineNumber != currentLineNumber)
                        {
                            // Separate ifs to avoid setting stopReason
                            if (ShouldEarlyExitFromStepping(out int earlyStoppedThread, out stopReason))
                            {
                                // Stop on a specific thread
                                stoppedThreadId = earlyStoppedThread;
                                break;
                            }
                        }
                    }

                    // Ensures we exit only if we actually stepped our thread id
                    exitLoop = exitLoop && (int)_interpreter.GetCurrentThreadId() == threadId;
                }

                if (_interpreter.State == InterpreterState.Exited)
                    ExecutionEnd?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        private bool ShouldEarlyExitFromStepping(out int stoppedThread, out StoppedEvent.ReasonValue? stopReason)
        {
            stoppedThread = -1;
            stopReason = null;

            // Check for function breakpoints
            foreach (NebulaBreakpoint fBp in Breakpoints.FunctionBreakpoints)
            {
                int threadId = _interpreter.AnyFrameJustStarted(fBp.Namespace, fBp.FuncName);
                if (threadId >= 0)
                {
                    stoppedThread = threadId;
                    stopReason = StoppedEvent.ReasonValue.FunctionBreakpoint;
                    return true;
                }
            }

            foreach (ConcurrentHashSet<NebulaBreakpoint> breakpointSet in Breakpoints.GenericBreakpoints)
            {
                foreach (NebulaBreakpoint bp in breakpointSet)
                {
                    int threadId = _interpreter.AnyFrameAt(bp.Namespace, bp.FuncName, bp.Line);
                    if (threadId >= 0)
                    {
                        stoppedThread = threadId;
                        stopReason = StoppedEvent.ReasonValue.Breakpoint;
                        return true;
                    }
                }
            }

            return stoppedThread >= 0;
        }

        /// <summary>
        /// Returns true if the line matches a debuggable opcode within the namespace. <br/>
        /// out string is name of debugged function and offsetLine is opcode index for the function to break at
        /// </summary>
        public bool IsLineDebuggable(string @namespace, int lineNumber, out string targetFuncionName, out int instructionIndex)
        {
            targetFuncionName = string.Empty;
            instructionIndex = -1;
            lineNumber--; // Debug files are offset by one
            if (DebugFiles.TryGetValue(@namespace, out var dbgFile))
            {
                DebugFunction? targetFunc = dbgFile.Functions.Values.FirstOrDefault(f =>
                {
                    int startLine = f.LineNumber;
                    int endLine = startLine + f.InstructionLines.Count;

                    return lineNumber >= startLine && lineNumber <= endLine;
                });

                if (targetFunc is null)
                    return false;

                targetFuncionName = targetFunc.Name;
                instructionIndex = targetFunc.InstructionLines.IndexOf(lineNumber);
                return true;
            }

            return false;
        }

        #endregion

        #region API

        public bool LoadScripts(ICollection<string> scripts)
        {
            if (!_interpreter.AddScripts(scripts))
            {
                _logger.LogError("Could not add scripts to debug");
                return false;
            }

            foreach (string script in scripts)
            {
                string dbgPath = Path.ChangeExtension(script, ".ndbg");
                if (!File.Exists(dbgPath))
                {
                    _logger.LogWarning($"Could not find debug data for script '{script}'");
                    continue;
                }

                SourceCode source = SourceCode.From(script);
                DebugFile file = DebugFile.LoadFromFile(dbgPath);

                string dbgContents = File.ReadAllText(dbgPath);
                _dbgFiles.Add(file.Namespace, file);
                DebugFilesChanged?.Invoke(this, EventArgs.Empty);

                _logger.LogInformation($"Loaded source and debug info of script '{script}'");

                // The ex
                string sourceFilePath = Path.GetDirectoryName(dbgPath)!;
                sourceFilePath = Path.Combine(sourceFilePath, file.OriginalFileName);

                DAPSources.Add(file.Namespace, new()
                {
                    Name = Path.GetFileName(file.OriginalFileName),
                    Path = sourceFilePath,
                });
            }

            return true;
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private
        internal void ReloadStateInformation()
        {
            int threadCount = _interpreter.ThreadCount;
            // Currently threads have no information, cheap trick here!
            _latestStateInformation = new(this);
            for (int i = 0; i < threadCount; i++)
            {
                var stackFrame = _interpreter.GetStackFrameOf(i);
                if (stackFrame == null)
                    continue;

                List<StackFrameW> stackTrace = stackFrame.ToList();
                List<NebulaStackFrame> actualFrames = [];
                foreach (StackFrameW? st in stackTrace)
                {
                    NebulaStackFrame newFrame = new(_latestStateInformation, st);
                    actualFrames.Add(newFrame);
                }

                NebulaThread thread = new(_latestStateInformation, i, actualFrames);
                _latestStateInformation.Threads.Add(i, thread);
            }
        }

        internal int GetLineNumber(NebulaStackFrame f)
        {
            return GetDebugInfo(f)!
                .InstructionLines[f.NativeFrame.CurrentInstructionIndex];
        }

        internal DebugFunction? GetDebugInfo(NebulaStackFrame f)
        {
            return DebugFiles[f.NativeFrame.Namespace]
                .Functions[f.NativeFrame.FunctionName];
        }

        #endregion
    }
}
