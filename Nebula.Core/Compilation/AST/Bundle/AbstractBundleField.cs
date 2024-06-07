namespace Nebula.Core.Binding.Symbols
{
    public sealed class AbstractBundleField(TypeSymbol fieldType, string fieldName, int ordinalPosition)
    {
        public TypeSymbol FieldType { get; } = fieldType;
        public string FieldName { get; } = fieldName;
        public int OrdinalPosition { get; } = ordinalPosition;
    }
}
