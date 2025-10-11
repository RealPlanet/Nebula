using Nebula.Commons.Debugger;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    /// <summary> Rapresents a group of variables. This can be a function scope, bundle data or array </summary>
    public class ScopeState
    {
        public VirtualMachineState Parent { get; }

        /// <summary> The ID of this scope </summary>
        public int VarReference { get; }

        /// <summary> Name of this scope </summary>
        public string Name { get; }

        /// <summary> False if this scope is a child of another scope </summary>
        public bool IsRootScope { get; }

        public virtual IReadOnlyList<IScopeNode> Children
        {
            get
            {
                _children ??= [];
                return _children;
            }
        }

        protected List<IScopeNode>? _children;
        protected readonly HashSet<Variable> _childNativeVariables = [];

        public ScopeState(VirtualMachineState parent, string name, bool isRootScope)
        {
            Parent = parent;
            VarReference = Parent.GetNextScopeId();
            Name = name;
            IsRootScope = isRootScope;
            Parent.AddScope(this);
        }

        internal void Add(DebugVariable? debugInfo, Variable variable)
        {
            _children ??= [];

            if (_childNativeVariables.Contains(variable))
            {
                return;
            }

            _childNativeVariables.Add(variable);
            if (variable.Type != Interop.Enumerators.TypeIdentifier.Bundle)
            {
                _children.Add(new VariableState(this, debugInfo?.Name ?? "???", variable));
                return;
            }

            _children.Add(new BundleScopeState(Parent, this, debugInfo, (Bundle)variable.Value));
        }
    }
}
