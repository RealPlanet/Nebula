using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
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
