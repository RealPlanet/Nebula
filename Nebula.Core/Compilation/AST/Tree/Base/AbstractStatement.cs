using Nebula.Commons.Syntax;

namespace Nebula.Core.Compilation.AST.Tree.Base
{
    public abstract class AbstractStatement
        : AbstractNode
    {
        protected AbstractStatement(Node syntax)
            : base(syntax) { }
    }

}
