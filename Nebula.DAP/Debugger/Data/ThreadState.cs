using Nebula.Commons.Debugger;
using System;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class ThreadState
    {
        public VirtualMachineState Parent { get; }
        public int ThreadId { get; }
        public IReadOnlyDictionary<int, FrameState> Callstack => _callstack;

        private readonly Dictionary<int, FrameState> _callstack = [];

        public ThreadState(VirtualMachineState parent, int threadId)
        {
            Parent = parent;
            ThreadId = threadId;
        }

        internal void AddFrame(FrameState state)
        {
            _callstack[state.FrameId] = state;
        }

        internal int GetLineNumber(FrameState frameState)
        {
            return Parent.GetLineNumber(frameState);
        }

        internal DebugFunction? GetDebugInfo(FrameState frameState)
        {
            return Parent.GetDebugInfo(frameState);
        }
    }
}
