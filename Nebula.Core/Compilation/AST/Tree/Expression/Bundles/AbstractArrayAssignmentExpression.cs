using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractArrayAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.ArrayAssignmentExpression;

        public AbstractExpression Target { get; }
        public AbstractExpression IndexExpression { get; }
        public AbstractExpression Expression { get; }

        public AbstractArrayAssignmentExpression(Node syntax, AbstractExpression target, AbstractExpression indexExpression, AbstractExpression expression)
            : base(syntax)
        {
            Target = target;
            IndexExpression = indexExpression;
            Expression = expression;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return IndexExpression;
            yield return Expression;
        }
    }
}
