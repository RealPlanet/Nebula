using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Nebula.Commons.Text;
using Nebula.Debugger.Bridge.Objects;
using Nebula.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Nebula.Debugger.Bridge
{

    public class NebulaDebugger
        : IDisposable
    {
        #region Properties

        internal IDictionary<int, NebulaThread> Threads => _latestStateInformation.Threads;
        internal DebuggerConfiguration Configuration { get; }
        internal Dictionary<string, DebugFile> DebugFiles { get; } = [];
        internal BreakpointManager Breakpoints { get; }
        internal InterpreterState VmState => _interpreter.State;
        internal Dictionary<string, Source> DAPSources { get; } = new();

        #endregion

        #region Fields
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly InterpreterW _interpreter;
        private readonly ILogger _logger;
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
        }

        #region Initialization

        public void Init(bool pauseOnStart)
        {
            _interpreter.Init(pauseOnStart);
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

        public bool Step(int threadId)
        {
            if (VmState != InterpreterState.Paused)
                return false;


            // Step is meant to skip one source line but native step means one opcode, we need to figure out how many opcodes to skip
            CallStackW stack = _interpreter.GetStackFrameOf(threadId);
            StackFrameW frame = stack.LastFrame;
            DebugFile currentDbgInfo = DebugFiles[frame.Namespace];
            DebugFunction currentDbgFunction = currentDbgInfo.Functions[frame.FunctionName];
            int currentOpcode = frame.CurrentInstructionIndex;

            int numberOfSteps = 1;
            int lineNumber = currentDbgFunction.InstructionLines[currentOpcode];
            if (currentOpcode < currentDbgFunction.InstructionLines.Count)
            {
                while (currentDbgFunction.InstructionLines[currentOpcode + numberOfSteps] == lineNumber &&
                    currentOpcode < currentDbgFunction.InstructionLines.Count)
                {
                    numberOfSteps++;
                }
            }

            while (numberOfSteps > 0)
            {
                _interpreter.Step();
                numberOfSteps--;
            }

            return true;
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
                DebugFiles.Add(file.Namespace, file);

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
                List<StackFrameW> stackTrace = _interpreter.GetStackFrameOf(i).ToList();
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
