using Nebula.Core.Compilation.AST.Symbols;

namespace Nebula.Core.Compilation.AST.Bundle
{
    public sealed class AbstractBundleField(TypeSymbol type, string fieldName, int ordinalPosition)
    {
        public TypeSymbol Type { get; } = type;
        public string FieldName { get; } = fieldName;
        public int OrdinalPosition { get; } = ordinalPosition;
    }
}
