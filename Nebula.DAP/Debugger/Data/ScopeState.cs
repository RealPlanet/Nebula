using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class ScopeState
    {
        public VirtualMachineState Parent { get; }
        public FrameState Frame { get; }
        public int ScopeId { get; }
        public string Name { get; }
        public IReadOnlyList<VariableState> Variables => _variables;

        private readonly List<VariableState> _variables = [];

        public ScopeState(VirtualMachineState parent, FrameState frame, string name)
        {
            Parent = parent;
            Frame = frame;
            ScopeId = Parent.GetNextScopeId();
            Name = name;

            Parent.AddScope(this);
        }

        internal void Add(string varName, Variable variable)
        {
            _variables.Add(new(this, varName, variable));
        }
    }
}
