using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractDefaultInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType { get; }
        public override AbstractNodeType Type => AbstractNodeType.DefaultInitializationExpression;
        public object Value => this;

        public AbstractDefaultInitializationExpression(Node syntax, object value)
            : base(syntax)
        {
            ResultType = TypeSymbol.BaseObject;
        }
    }
}
