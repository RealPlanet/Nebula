using Nebula.Interop.Enumerators;
using Nebula.Interop.Structures;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class VariableState
    {
        public ScopeState Parent { get; }

        public string Name { get; }

        public Variable OriginalVariable { get; }

        public TypeIdentifier Type { get; }

        public object? Value { get; }

        public VariableState(ScopeState parent, string name, Variable variable)
        {
            Parent = parent;
            Name = name;
            OriginalVariable = variable;
            Type = variable.Type;
            Value = variable.Value ?? "<empty>";
        }
    }
}
