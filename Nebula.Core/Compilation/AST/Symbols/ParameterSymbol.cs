using Nebula.Core.Compilation.AST.Symbols.Base;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class ParameterSymbol
        : LocalVariableSymbol
    {
        public override SymbolType SymbolType => SymbolType.Parameter;
        public int OrdinalPosition { get; }
        public ParameterSymbol(string name, TypeSymbol type, int ordinalPosition)
            : base(name, isReadOnly: false, type, null)
        {
            OrdinalPosition = ordinalPosition;
        }
    }
}
