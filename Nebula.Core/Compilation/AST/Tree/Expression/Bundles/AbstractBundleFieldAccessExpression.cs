using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    /// <summary>
    /// Access the field and returns the field data
    /// </summary>
    public sealed class AbstractBundleFieldAccessExpression
        : AbstractVariableExpression
    {
        public AbstractBundleField Field { get; }
        public override TypeSymbol ResultType => Field.FieldType;
        public AbstractBundleFieldAccessExpression(Node syntax, VariableSymbol bundleVariable, AbstractBundleField field)
            : base(syntax, bundleVariable)
        {
            Field = field;
        }
    }
}
