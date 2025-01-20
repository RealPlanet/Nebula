using Microsoft.Extensions.Logging;
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
        private readonly HashSet<int> _debuggableLines = new();
        private StateInformation _latestStateInformation;
        private Dictionary<string, DebugFile> _dbgFiles = [];
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

            DebugFilesChanged += ReloadLineNumberChace;
        }

        private void ReloadLineNumberChace(object? sender, EventArgs e)
        {
            _debuggableLines.Clear();
            var lines = _dbgFiles.Values.SelectMany(s => s.Functions.Values)
                .SelectMany(v => v.InstructionLines);
            foreach (var l in lines)
                _debuggableLines.Add(l);
        }

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

        public void Pause()
        {
            _interpreter.Pause();
            ReloadStateInformation();
        }

        public bool Step(int threadId, bool isStepIn)
        {
            // Continue step
            if(threadId < 0)
            {
                _interpreter.Step();

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
                    while (currentDbgFunction.InstructionLines[currentOpcode + numberOfSteps] == lineNumber &&
                        currentOpcode < frame.InstructionCount)
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

                    // Ensures we exit only if we actually stepped our thread id
                    exitLoop = exitLoop && (int)_interpreter.GetCurrentThreadId() == threadId;
                }

                if (_interpreter.State == InterpreterState.Exited)
                    ExecutionEnd?.Invoke(this, EventArgs.Empty);
            }

            return true;
        }

        public bool IsLineDebuggable(int lineNumber)
        {
            return _debuggableLines.Contains(lineNumber);
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
                    Name = Path.GetFileNameWithoutExtension(file.OriginalFileName),
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
