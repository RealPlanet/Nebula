using Nebula.Interop.Structures;

namespace Nebula.Debugger.Bridge.Objects
{
    public class NebulaVariable
        : BaseNebulaDebuggerObejct
    {
        public NebulaScope ParentScope { get; }
        public Variable NativeVariable { get; }
        public int ScopeId => ParentScope.ScopeId;
        public string Name { get; } = "<UNKNOWN>";
        public string Value { get; }
        public string Type { get; }

        public NebulaVariable(StateInformation parent, NebulaScope parentScope, string varName, Variable variable)
            : base(parent)
        {
            ParentScope = parentScope;
            NativeVariable = variable;

            Name = varName;
            Type = NativeVariable.Type.ToString();
            Value = NativeVariable.Value?.ToString() ?? "<NO_VAL>";
        }
    }
}
