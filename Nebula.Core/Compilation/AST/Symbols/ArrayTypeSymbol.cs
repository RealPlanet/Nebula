namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class ArrayTypeSymbol
        : TypeSymbol
    {
        public TypeSymbol ValueType { get; }
        public int Rank { get; }

        public ArrayTypeSymbol(TypeSymbol valueType, int rank)
            : base(BaseArray.Name)
        {
            ValueType = valueType;
            Rank = rank;
        }
    }
}
