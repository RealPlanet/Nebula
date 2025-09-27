using Nebula.Interop.Enumerators;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class ScopeState
    {
        public VirtualMachineState Parent { get; }
        public int VarReference { get; }
        public string Name { get; }
        public IReadOnlyList<IScopeNode> Children => _children;

        private readonly List<IScopeNode> _children = [];
        private readonly HashSet<Variable> _childNativeVariables = [];

        public ScopeState(VirtualMachineState parent, string name)
        {
            Parent = parent;
            VarReference = Parent.GetNextScopeId();
            Name = name;

            Parent.AddScope(this);
        }

        public ScopeState(VirtualMachineState parent, string name, Bundle source)
            : this(parent, name)
        {

        }

        internal void Add(string varName, Variable variable)
        {
            if(_childNativeVariables.Contains(variable))
            {
                return;
            }

            _childNativeVariables.Add(variable);
            if (variable.Type != Interop.Enumerators.TypeIdentifier.Bundle)
            {
                _children.Add(new VariableState(this, varName, variable));
                return;
            }

            _children.Add(new BundleState(Parent, this, varName, variable));
        }
    }

    public class BundleState
        : IScopeNode
    {
        public int VarReference { get; }
        public string Name { get; }
        public object? Value { get; }

        public IReadOnlyList<IScopeNode> Children => _children;
        public VirtualMachineState Parent { get; }
        public ScopeState ScopeParent { get; }
        public TypeIdentifier ValueType { get; } = TypeIdentifier.Bundle;
        public bool CanOverrideValue { get; } = false;

        private readonly List<IScopeNode> _children = [];
        private bool _initDone = false;

        public BundleState(VirtualMachineState parent,
                           ScopeState scopeParent,
                           string variableName,
                           Variable bundleVariable)
        {
            Name = variableName;
            Parent = parent;
            ScopeParent = scopeParent;
        }

        public bool OverrideValue(string value) => false;
    }
}
