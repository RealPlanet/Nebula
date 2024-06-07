using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractConditionalGotoStatement
        : AbstractStatement
    {
        public AbstractLabel Label { get; }
        public AbstractExpression Condition { get; }
        public bool JumpIfTrue { get; }
        public override AbstractNodeType Type => AbstractNodeType.ConditionalGotoStatement;
        public AbstractConditionalGotoStatement(Node syntax, AbstractLabel label, AbstractExpression condition, bool jumpIfTrue = true)
            : base(syntax)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }
    }
}
