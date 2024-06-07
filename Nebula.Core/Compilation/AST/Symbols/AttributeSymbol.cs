using Nebula.CodeEmitter;
using System;
using System.Collections.Generic;

namespace Nebula.Core.Binding.Symbols
{
    public sealed class AttributeSymbol
        : Symbol
    {
        public override SymbolType SymbolType => SymbolType.Attribute;
        public object? Attribute { get; } = null;
        public bool IsMethodAttribute => Attribute is NativeAttribute;
        public bool CanHaveReturnType { get; } = true;
        public bool CanHaveParameters { get; } = true;

        private AttributeSymbol(string name, bool canHaveReturnType, bool canHaveParameters)
            : base(name)
        {
            CanHaveReturnType = canHaveReturnType;
            CanHaveParameters = canHaveParameters;
            if (Enum.TryParse<NativeAttribute>(name, ignoreCase: true, out NativeAttribute result))
            {
                Attribute = result;
                return;
            }
        }

        private static readonly Dictionary<string, AttributeSymbol> _symbols = new(StringComparer.OrdinalIgnoreCase)
        {
            { NativeAttribute.AutoExec.ToString(), new(NativeAttribute.AutoExec.ToString(), false, false)},
        };

        public static AttributeSymbol FromName(string name)
        {
            if (_symbols.TryGetValue(name, out AttributeSymbol? symbol))
            {
                return symbol;
            }

            return new(name, true, true);
        }
    }
}
