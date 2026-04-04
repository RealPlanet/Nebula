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
        public enum FieldMode
        {
            Read,
            Write,
        }

        public override AbstractNodeType Type => AbstractNodeType.ObjectFieldAccessExpression;
        public override TypeSymbol ResultType => Field.FieldType;
        public AbstractBundleField Field { get; }
        public FieldMode Mode { get; set; } = FieldMode.Read;

        public AbstractObjectFieldAccessExpression(Node syntax, AbstractBundleField field)
            : base(syntax)
        {
            Field = field;
        }
    }
}
