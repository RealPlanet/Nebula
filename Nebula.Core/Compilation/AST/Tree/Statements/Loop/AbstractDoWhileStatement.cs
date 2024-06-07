using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractDoWhileStatement
        : AbstractLoopStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.DoWhileStatement;
        public AbstractStatement Body { get; }
        public AbstractExpression Condition { get; }

        public AbstractDoWhileStatement(Node syntax, AbstractStatement body, AbstractExpression condition, AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax, breakLabel, continueLabel)
        {
            Body = body;
            Condition = condition;
        }
    }
}
