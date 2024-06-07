using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractExpressionStatement
        : AbstractStatement
    {
        public AbstractExpression Expression { get; }
        public override AbstractNodeType Type => AbstractNodeType.ExpressionStatement;
        public AbstractExpressionStatement(Node syntax, AbstractExpression expression)
            : base(syntax)
        {
            Expression = expression;
        }
    }
}
