namespace Nebula.Core.Binding.Symbols
{
    public sealed class ArrayTypeSymbol
        : TypeSymbol
    {
        public TypeSymbol ValueType { get; }
        public int Rank { get; }

        public ArrayTypeSymbol(TypeSymbol valueType, int rank)
            : base(TypeSymbol.BaseArray.Name)
        {
            ValueType = valueType;
            Rank = rank;
        }
    }
}
