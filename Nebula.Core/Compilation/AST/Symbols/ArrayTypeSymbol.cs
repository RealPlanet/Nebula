namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class ArrayTypeSymbol
        : TypeSymbol
    {
        public TypeSymbol ValueType { get; }

        public ArrayTypeSymbol(TypeSymbol valueType)
            : base(string.Empty, BaseArray.Name)
        {
            ValueType = valueType;

            _registeredFunctions.Add(new FunctionSymbol("Count", [], [], Int, null!));
            _registeredFunctions.Add(new FunctionSymbol("Append", [new ParameterSymbol("item", ValueType, 0)], [], Void, null!));
        }

        public override string ToString()
        {
            return $"{Name} - {ValueType}[]";
        }
    }
}
