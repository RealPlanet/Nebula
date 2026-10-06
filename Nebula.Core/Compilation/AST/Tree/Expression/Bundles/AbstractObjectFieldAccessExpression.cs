using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

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
        public override TypeSymbol ResultType => Field.Type;
        public AbstractExpression Target { get; }
        public AbstractBundleField Field { get; }
        public FieldMode Mode { get; set; } = FieldMode.Read;

        public AbstractObjectFieldAccessExpression(Node syntax, AbstractExpression target, AbstractBundleField field)
            : base(syntax)
        {
            Target = target;
            Field = field;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return Target;
        }
    }
}
