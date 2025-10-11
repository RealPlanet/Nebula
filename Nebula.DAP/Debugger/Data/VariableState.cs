using Nebula.Interop.Enumerators;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class VariableState
        : IScopeNode
    {

        public ScopeState Parent { get; }

        public string Name { get; }

        public Variable OriginalVariable { get; }

        public TypeIdentifier ValueType { get; }

        public object? Value => OriginalVariable.Value ?? "<empty>";

        public IReadOnlyList<IScopeNode> Children { get; } = [];
        public int VarReference => 0;

        public bool CanOverrideValue => ValueType != TypeIdentifier.Bundle && ValueType != TypeIdentifier.Array;

        public VariableState(ScopeState parent, string name, Variable variable)
        {
            Parent = parent;
            Name = name;
            OriginalVariable = variable;
            ValueType = variable.Type;
        }

        /// <summary>
        /// Handles type check before setting the value (or not) in the native virtual machine
        /// </summary>
        public bool OverrideValue(string value)
        {
            switch (ValueType)
            {
                case TypeIdentifier.Int32:
                    {
                        if (!int.TryParse(value, out int iValue))
                        {
                            return false;
                        }

                        return OriginalVariable.Set(iValue);
                    }
                case TypeIdentifier.Float:
                    {
                        if (!float.TryParse(value, out float fValue))
                        {
                            return false;
                        }

                        return OriginalVariable.Set(fValue);
                    }
                case TypeIdentifier.String:
                    {
                        return OriginalVariable.Set(value);
                    }
                case TypeIdentifier.Bundle:
                case TypeIdentifier.Array:
                case TypeIdentifier.Void:
                case TypeIdentifier.Unknown:
                default:
                    return true;
            }
        }
    }
}
