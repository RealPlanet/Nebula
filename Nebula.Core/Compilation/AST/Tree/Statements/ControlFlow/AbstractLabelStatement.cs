using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public sealed class AbstractLabelStatement
        : AbstractStatement
    {
        public AbstractLabel Label { get; }
        public override AbstractNodeType Type => AbstractNodeType.LabelStatement;
        public AbstractLabelStatement(Node syntax, AbstractLabel label)
            : base(syntax)
        {
            Label = label;
        }
    }
}
