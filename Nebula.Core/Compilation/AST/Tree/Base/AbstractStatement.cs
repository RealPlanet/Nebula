using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public abstract class AbstractStatement
        : AbstractNode
    {
        protected AbstractStatement(Node syntax)
            : base(syntax) { }
    }

}
