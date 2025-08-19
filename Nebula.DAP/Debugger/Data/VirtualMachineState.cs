using Nebula.Commons.Debugger;
using System;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class VirtualMachineState
    {
        public IReadOnlyDictionary<int, ThreadState> Threads => _threads;


        private readonly NebulaDebugger _parent;
        private readonly Dictionary<int, ThreadState> _threads = [];
        private int _frameIdCounter = 0;
        private int _scopeIdCounter = 0;

        public VirtualMachineState(NebulaDebugger parent)
        {
            _parent = parent;
        }

        internal int GetNextFrameId()
            => _frameIdCounter++;

        internal int GetNextScopeId()
            => _scopeIdCounter++;


        internal void AddThread(ThreadState state)
        {
            _threads[state.ThreadId] = state;
        }

        internal int GetLineNumber(FrameState frameState)
        {
            int opcode = frameState.OriginalFrame.NextInstructionIndex;
            DebugFunction? debugFunc = _parent.GetDebugInfo(frameState.FunctionNamespace, frameState.FunctionName);

            if (debugFunc is null)
            {
                return 0;
            }

            if (opcode >= 0)
            {
                if (debugFunc.DeltaInstructionLines.TryGetValue(opcode, out var line))
                {
                    return line;
                }

                var lastLine = -1;
                foreach (var kvp in debugFunc.DeltaInstructionLines)
                {
                    if (kvp.Key < opcode)
                    {
                        lastLine = kvp.Value;
                        continue;
                    }

                    if (kvp.Key >= opcode)
                    {
                        return lastLine;
                    }
                }

                return lastLine;
            }

            return debugFunc.LineNumber;
        }

        internal FrameState? GetFrameById(int frameId)
        {
            foreach (var t in Threads.Values)
            {
                if (t.Callstack.TryGetValue(frameId, out var frame))
                {
                    return frame;
                }
            }

            return null;
        }

        internal DebugFunction? GetDebugInfo(FrameState frameState)
        {
            return _parent.GetDebugInfo(frameState.FunctionNamespace, frameState.FunctionName);
        }
    }
}
