using Nebula.Interop.Enumerators;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public interface IScopeNode
    {
        int VarReference { get; }

        string Name { get; }

        TypeIdentifier ValueType { get; }

        object? Value { get; }

        bool CanOverrideValue { get; }

        bool OverrideValue(string value);

        IReadOnlyList<IScopeNode> Children { get; }
    }
}
