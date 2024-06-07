using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractNopStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.NopStatement;

        public AbstractNopStatement(Node syntax)
            : base(syntax) { }
    }
}
