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

            _registeredFunctions.Add(new FunctionSymbol("Count", [], [], Int, null));
            _registeredFunctions.Add(new FunctionSymbol("Append", [new ParameterSymbol("item", ValueType, 0)], [], Void, null));
        }

        public override string ToString()
        {
            return $"{Name}[]";
        }
    }
}
