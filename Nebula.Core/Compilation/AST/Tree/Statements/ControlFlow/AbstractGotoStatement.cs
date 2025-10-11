using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow
{
    public sealed class AbstractGotoStatement
        : AbstractStatement
    {
        public AbstractLabel Label { get; }
        public override AbstractNodeType Type => AbstractNodeType.GotoStatement;
        public AbstractGotoStatement(Node syntax, AbstractLabel label)
            : base(syntax)
        {
            Label = label;
        }
    }
}
