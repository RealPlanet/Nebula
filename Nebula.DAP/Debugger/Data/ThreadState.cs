using Nebula.Commons.Debugger;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class ThreadState
    {
        public VirtualMachineState Parent { get; }
        public int ThreadId { get; }
        public Callstack OriginalCallstack { get; }

        public IReadOnlyDictionary<int, FrameState> Frames => _frameDictionary;
        public IReadOnlyList<FrameState> CallStack => _callstack;

        private readonly Dictionary<int, FrameState> _frameDictionary = [];
        private readonly List<FrameState> _callstack = [];

        public ThreadState(VirtualMachineState parent, int threadId, Callstack originalCallstack)
        {
            Parent = parent;
            ThreadId = threadId;
            OriginalCallstack = originalCallstack;
        }

        internal void AddFrame(FrameState state)
        {
            _frameDictionary[state.FrameId] = state;
            _callstack.Add(state);
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
