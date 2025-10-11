using Nebula.Commons.Debugger;
using Nebula.Interop.Enumerators;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public class BundleScopeState
        : ScopeState, IScopeNode
    {
        public override IReadOnlyList<IScopeNode> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = [];
                    for (int i = 0; i < _bundleRef.Fields.Count; i++)
                    {
                        Variable? variable = _bundleRef.Fields[i];
                        DebugVariable? dbgInfo = null;

                        if (_bundleDebugInfo != null)
                        {
                            dbgInfo = _bundleDebugInfo.Fields[i];
                        }

                        Add(dbgInfo, variable);
                    }
                }

                return _children;
            }
        }

        public object? Value { get; }
        public ScopeState ScopeParent { get; }
        public TypeIdentifier ValueType { get; } = TypeIdentifier.Bundle;
        public bool CanOverrideValue { get; } = false;

        private readonly Bundle _bundleRef;
        private readonly DebugBundleDefinition? _bundleDebugInfo;

        public BundleScopeState(VirtualMachineState parent,
                           ScopeState scopeParent,
                           DebugVariable? debugInfo,
                           Bundle bundleVariable)
            : base(parent, debugInfo?.Name ?? "Unknown bundle", false)
        {
            ScopeParent = scopeParent;
            _bundleRef = bundleVariable;
            if (debugInfo != null)
            {
                _bundleDebugInfo = parent.GetObjectDebugInfo(debugInfo.SourceNamespace ?? string.Empty, debugInfo.SourceType);
            }
        }

        public bool OverrideValue(string value) => false;
    }
}
