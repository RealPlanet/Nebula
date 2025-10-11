using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements.Loop
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
