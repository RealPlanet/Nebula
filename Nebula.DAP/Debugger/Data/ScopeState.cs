using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class ScopeState
    {
        public FrameState Parent { get; }
        public int ScopeId { get; }
        public string Name { get; }
        public IReadOnlyDictionary<string, VariableState> Variables => _variables;

        private readonly Dictionary<string, VariableState> _variables = [];

        public ScopeState(FrameState parent, string name)
        {
            Parent = parent;
            ScopeId = parent.GetNextScopeId();
            Name = name;
        }

        internal void Add(string varName, Variable variable)
        {
            _variables[varName] = new(this, varName, variable);
        }
    }
}
