using Nebula.Commons.Debugger;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class VirtualMachineState
    {
        public IReadOnlyDictionary<int, ThreadState> Threads => _threads;
        public IReadOnlyDictionary<int, FrameState> Frames => _frames;
        public IReadOnlyDictionary<int, ScopeState> Scopes => _scopes;

        private readonly NebulaDebugger _parent;
        private readonly Dictionary<int, ThreadState> _threads = [];
        private readonly Dictionary<int, FrameState> _frames = [];
        private readonly Dictionary<int, ScopeState> _scopes = [];

        private int _frameIdCounter = 1;
        private int _scopeIdCounter = 1;
        private int _variableIdCounter = 1;

        public VirtualMachineState(NebulaDebugger parent)
        {
            _parent = parent;
        }

        internal int GetNextFrameId()
            => _frameIdCounter++;

        internal int GetNextScopeId()
            => _scopeIdCounter++;

        internal int GetNextVariableId()
            => _variableIdCounter++;

        internal void AddThread(ThreadState state)
        {
            _threads[state.ThreadId] = state;
        }

        internal void AddScope(ScopeState s)
        {
            _scopes[s.ScopeId] = s;
        }

        internal int GetLineNumber(FrameState frameState)
        {
            int opcode = frameState.OriginalFrame.NextInstructionIndex;
            DebugFunction? debugFunc = _parent.GetDebugInfo(frameState.FunctionNamespace, frameState.FunctionName);

            if (debugFunc is null)
            {
                return 0;
            }

            return GetLineNumber(debugFunc, opcode);
        }

        internal static int GetLineNumber(DebugFunction debugFunc, int currentOpcode)
        {
            if (currentOpcode >= 0)
            {
                int lastLine = -1;
                foreach (KeyValuePair<int, int> kvp in debugFunc.LineStartingOpcodeIndex)
                {
                    if(kvp.Value == currentOpcode)
                    {
                        return kvp.Key;
                    }

                    if (kvp.Value < currentOpcode)
                    {
                        lastLine = kvp.Key;
                        continue;
                    }

                    if (kvp.Value >= currentOpcode)
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
            foreach (ThreadState t in Threads.Values)
            {
                if (t.Frames.TryGetValue(frameId, out FrameState? frame))
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
