using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractReturnStatement
        : AbstractStatement
    {
        public AbstractExpression? Expression { get; }
        public override AbstractNodeType Type => AbstractNodeType.ReturnStatement;
        public AbstractReturnStatement(Node syntax, AbstractExpression? expression)
            : base(syntax)
        {
            Expression = expression;
        }
    }
}
