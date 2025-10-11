using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    /// <summary> Access the field and returns the field data </summary>
    public sealed class AbstractObjectFieldAccessExpression
        : AbstractExpression
    {
        public override AbstractNodeType Type => AbstractNodeType.ObjectFieldAccessExpression;
        public override TypeSymbol ResultType => Field.FieldType;
        public AbstractExpression Target { get; }
        public AbstractBundleField Field { get; }

        public AbstractObjectFieldAccessExpression(Node syntax, AbstractExpression target, AbstractBundleField field)
            : base(syntax)
        {
            Target = target;
            Field = field;
        }
    }
}
