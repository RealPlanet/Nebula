using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public abstract class AbstractLoopStatement : AbstractStatement
    {
        public AbstractLabel BreakLabel { get; }
        public AbstractLabel ContinueLabel { get; }
        protected AbstractLoopStatement(Node syntax, AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
    }

}
