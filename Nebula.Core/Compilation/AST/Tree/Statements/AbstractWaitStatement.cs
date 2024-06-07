using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractWaitStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WaitStatement;

        public AbstractExpression TimeExpression { get; }

        public AbstractWaitStatement(Node syntax, AbstractExpression time)
            : base(syntax)
        {
            TimeExpression = time;
        }
    }
}
