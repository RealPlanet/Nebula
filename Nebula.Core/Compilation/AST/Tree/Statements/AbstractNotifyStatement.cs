using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractNotifyStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.NotifyStatement;

        public AbstractExpression BundleToNotifyFrom { get; }
        public AbstractExpression NotifyExpression { get; }

        public AbstractNotifyStatement(Node syntax, AbstractExpression bundleNotifier, AbstractExpression notifyExpr) 
            : base(syntax)
        {
            BundleToNotifyFrom = bundleNotifier;
            NotifyExpression = notifyExpr;
        }
    }

}
