namespace Nebula.Core.Binding.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolType SymbolType => SymbolType.LocalVariable;
        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(name, isReadOnly, variableType, constant)
        {
        }
    }
}
