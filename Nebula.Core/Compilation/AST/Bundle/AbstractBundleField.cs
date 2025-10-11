using Nebula.Core.Compilation.AST.Symbols;

namespace Nebula.Core.Compilation.AST.Bundle
{
    public sealed class AbstractBundleField(TypeSymbol fieldType, string fieldName, int ordinalPosition)
    {
        public TypeSymbol FieldType { get; } = fieldType;
        public string FieldName { get; } = fieldName;
        public int OrdinalPosition { get; } = ordinalPosition;
    }
}
