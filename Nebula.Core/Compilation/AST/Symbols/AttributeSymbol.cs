using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Interop.Enumerators;
using System;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class AttributeSymbol
        : Symbol
    {
        public override SymbolType SymbolType => SymbolType.Attribute;
        public object? Attribute { get; } = null;
        public bool IsMethodAttribute => Attribute is AttributeType;
        public bool CanHaveReturnType { get; } = true;
        public bool CanHaveParameters { get; } = true;

        private AttributeSymbol(string name, bool canHaveReturnType, bool canHaveParameters)
            : base(name)
        {
            CanHaveReturnType = canHaveReturnType;
            CanHaveParameters = canHaveParameters;
            if (Enum.TryParse<AttributeType>(name, ignoreCase: true, out AttributeType result))
            {
                Attribute = result;
                return;
            }
        }

        private static readonly Dictionary<string, AttributeSymbol> _symbols = new(StringComparer.OrdinalIgnoreCase)
        {
            { AttributeType.AutoExec.ToString(), new(AttributeType.AutoExec.ToString(), false, false)},
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
