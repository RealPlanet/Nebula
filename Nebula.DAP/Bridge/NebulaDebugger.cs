using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Nebula.Commons.Debugger;
using Nebula.Commons.Text;
using Nebula.Debugger.Bridge.Objects;
using Nebula.Interop.SafeHandles;
using Nebula.Interop.Structures;
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
        public event EventHandler<string>? OnStdOut;
        #endregion

        #region Properties

        internal IDictionary<int, NebulaThread> Threads => _latestStateInformation.Threads;
        internal DebuggerConfiguration Configuration { get; }
        internal IReadOnlyDictionary<string, DebugFile> DebugFiles => _dbgFiles;
        internal BreakpointManager Breakpoints { get; }
        internal VirtualMachine.State VmState => _interpreter.VMState;
        internal Dictionary<string, Source> DAPSources { get; } = new();

        #endregion

        #region Fields
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly VirtualMachine _interpreter;
        private readonly ILogger _logger;
        private readonly Dictionary<string, DebugFile> _dbgFiles = [];
        private StateInformation _latestStateInformation;
        #endregion

        public NebulaDebugger(DebuggerConfiguration configuration, ILogger logger)
        {
            Configuration = configuration;
            _logger = logger;
            _interpreter = new();
            _interpreter.RedirectStdOutput(OnStdOutWrite, OnStdOutLine);
            _latestStateInformation = new(this);
            Breakpoints = new BreakpointManager(this);

            _logger.LogInformation("Debugger constructed");
            //DebugFilesChanged += ReloadLineNumberCache;
        }

        private void OnStdOutLine(string message) => OnStdOut?.Invoke(this, $"{message}\n");
        private void OnStdOutWrite(string message) => OnStdOut?.Invoke(this, message);

        private void OnScriptParseError(string path, ReportType type, string message)
        {
            _logger.LogInformation($"[{path}] [{type}] {message}");
        }

        #region Initialization

        public void Init()
        {
            _interpreter.Initialize(true);
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
                int[] startingOpcodes = _interpreter.NextOpcodesOfAllThreads;

                _interpreter.Step();


                if (ShouldEarlyExitFromStepping(out int earlyStoppedThread, out stopReason))
                {
                    // Only consider early exits when we move at least one line for the stopping thread, otherwise we'll keep breaking on the same breakpoint
                    Callstack earlyStoppingStack = _interpreter.GetCallstackOfThread(earlyStoppedThread);
                    Interop.Structures.Frame frame = earlyStoppingStack.LastFrame;
                    DebugFile currentDbgInfo = DebugFiles[frame.Namespace];
                    DebugFunction currentDbgFunction = currentDbgInfo.Functions[frame.FunctionName];
                    int nextOpcodeIndex = frame.NextInstructionIndex;


                    int startingLine = currentDbgFunction.LineNumber;
                    if (startingOpcodes[earlyStoppedThread] >= 0)
                    {
                        startingLine = currentDbgFunction.InstructionLines[startingOpcodes[earlyStoppedThread]];
                    }

                    int steppedLine = currentDbgFunction.LineNumber;
                    if (nextOpcodeIndex >= 0)
                    {
                        steppedLine = currentDbgFunction.InstructionLines[nextOpcodeIndex];
                    }

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

                if (_interpreter.VMState == VirtualMachine.State.Exited)
                {
                    ExecutionEnd?.Invoke(this, EventArgs.Empty);
                }

                return true;
            }

            // Step is meant to skip one source line but native step means one opcode, we need to figure out how many opcodes to skip
            Callstack stack = _interpreter.GetCallstackOfThread(threadId);
            if (stack != null)
            {
                int stackCount = stack.FrameCount;
                Interop.Structures.Frame frame = stack.LastFrame;
                DebugFile currentDbgInfo = DebugFiles[frame.Namespace];
                DebugFunction currentDbgFunction = currentDbgInfo.Functions[frame.FunctionName];
                int nextOpcodeIndex = frame.NextInstructionIndex;

                int numberOfSteps = 0;
                int lineNumber = -1;
                if (nextOpcodeIndex >= 0)
                {
                    lineNumber = currentDbgFunction.InstructionLines[nextOpcodeIndex];
                }

                if (nextOpcodeIndex < currentDbgFunction.InstructionLines.Count)
                {
                    if (nextOpcodeIndex < 0)
                    {
                        // Func has not started yet
                        numberOfSteps++; // Skip just one
                    }
                    else
                    {
                        while (nextOpcodeIndex + numberOfSteps < frame.InstructionCount &&
                            currentDbgFunction.InstructionLines[nextOpcodeIndex + numberOfSteps] == lineNumber)
                        {
                            numberOfSteps++;
                        }
                    }
                }

                int targetOpcode = frame.NextInstructionIndex + numberOfSteps;
                // If we want to step in we'll never reach the target opcode because w'd have to step over the function call
                if (isStepIn)
                {
                    targetOpcode--;
                }

                bool exitLoop = false;
                while (!exitLoop &&
                    frame.NextInstructionIndex != targetOpcode &&
                    frame.NextInstructionIndex < frame.InstructionCount) // Happens when we do the last opcode
                {
                    // This condition triggers when we are about to execute a return statement in a function and thus de-allocating the frame
                    exitLoop = frame.NextInstructionIndex == targetOpcode &&
                        targetOpcode == frame.InstructionCount - 1;

                    _interpreter.Step();
                    Callstack latestStack = _interpreter.GetCallstackOfThread(threadId);
                    Interop.Structures.Frame? lastFrame = latestStack?.LastFrame;
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
                        int currentLineNumber = -1;
                        if (frame.NextInstructionIndex >= 0)
                        {
                            currentLineNumber = currentDbgFunction.InstructionLines[frame.NextInstructionIndex];
                        }

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
                    exitLoop = exitLoop && (int)_interpreter.CurrentThreadId == threadId;
                }

                if (_interpreter.VMState == VirtualMachine.State.Exited)
                {
                    ExecutionEnd?.Invoke(this, EventArgs.Empty);
                }
            }

            // TODO :: If we have the following
            // - 0 brfalse 2
            // - 1  exit
            // - 2 continue
            // And brfalse jumped next index will be 2 and current index will evaluate to 1
            // as a result debugger will show us inside the IF

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
                    int threadId = _interpreter.AnyFrameAboutToBeAt(bp.Namespace, bp.FuncName, bp.Line);
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
            if (DebugFiles.TryGetValue(@namespace, out DebugFile? dbgFile))
            {
                DebugFunction? targetFunc = dbgFile.Functions.Values.FirstOrDefault(f =>
                {
                    int startLine = f.LineNumber;
                    int endLine = startLine + f.InstructionLines.Count;

                    return lineNumber >= startLine && lineNumber <= endLine;
                });

                if (targetFunc is null)
                {
                    return false;
                }

                targetFuncionName = targetFunc.Name;
                instructionIndex = targetFunc.InstructionLines.IndexOf(lineNumber);
                return true;
            }

            return false;
        }

        #endregion

        #region API

        public bool InitDebugger(ICollection<string> scripts, string nativeDllBindings)
        {
            if (!_interpreter.AddScripts(scripts, OnScriptParseError))
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

            List<string> uniqueNativeFunctions = _dbgFiles.SelectMany(f => f.Value.NativeFunctions)
                .Distinct()
                .ToList();

            if (string.IsNullOrEmpty(nativeDllBindings))
            {
                if (uniqueNativeFunctions.Count > 0)
                {
                    // TODO Report error!
                    return false;
                }

                return true;
            }

            // TODO :: Error check!
            return _interpreter.LoadNativesFromDll(nativeDllBindings, uniqueNativeFunctions);
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
                Callstack callstack = _interpreter.GetCallstackOfThread(i);
                if (callstack == null)
                {
                    continue;
                }

                List<Frame> stackTrace = callstack.Frames.ToList();
                List<NebulaStackFrame> actualFrames = [];
                foreach (Frame? st in stackTrace)
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
            int opcode = f.NativeFrame.NextInstructionIndex;
            DebugFunction debugFunc = GetDebugInfo(f)!;
            if (opcode >= 0)
            {
                return debugFunc.InstructionLines[opcode];
            }

            return debugFunc.LineNumber;
        }

        internal DebugFunction? GetDebugInfo(NebulaStackFrame f)
        {
            return DebugFiles[f.NativeFrame.Namespace]
                .Functions[f.NativeFrame.FunctionName];
        }

        #endregion
    }
}
