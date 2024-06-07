using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractIfStatement
        : AbstractStatement
    {
        public AbstractExpression Condition { get; }
        public AbstractStatement ThenStatement { get; }
        public AbstractStatement? ElseStatement { get; }
        public override AbstractNodeType Type => AbstractNodeType.IfStatement;
        public AbstractIfStatement(Node syntax, AbstractExpression condition, AbstractStatement thenStatement, AbstractStatement? elseStatement)
            : base(syntax)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }
    }
}
