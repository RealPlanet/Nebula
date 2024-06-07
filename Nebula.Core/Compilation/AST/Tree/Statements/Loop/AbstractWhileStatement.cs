using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractWhileStatement
        : AbstractLoopStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WhileStatement;
        public AbstractExpression Condition { get; }
        public AbstractStatement Body { get; }
        public AbstractWhileStatement(Node syntax, AbstractExpression condition, AbstractStatement body,
            AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax, breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
        }
    }
}
