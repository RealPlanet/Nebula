using Nebula.Interop;
using System.Collections.Generic;

namespace Nebula.Debugger.Bridge.Objects
{
    public class NebulaScope
        : BaseNebulaDebuggerObejct
    {
        public string Name { get; }
        public int ScopeId { get; }
        public NebulaStackFrame ParentFrame { get; }
        public List<NebulaVariable> Variables { get; } = [];

        public NebulaScope(StateInformation parent, string name, NebulaStackFrame parentFrame)
            : base(parent)
        {
            ScopeId = parent.VariableReferenceCounter++;
            Name = name;
            ParentFrame = parentFrame;
        }

        internal void AddVariable(string varName, VariableW variableW)
        {
            Variables.Add(new NebulaVariable(Parent, this, varName, variableW));
        }
    }
}
