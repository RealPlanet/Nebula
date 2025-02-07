using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractWaitNotificationStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WaitNotificationStatement;

        public AbstractExpression BundleToWaitOn { get; }
        public AbstractExpression NotifyExpression { get; }
        public AbstractWaitNotificationStatement(Node syntax, AbstractExpression bundleNotifier, AbstractExpression notifyExpr)
            : base(syntax)
        {
            BundleToWaitOn = bundleNotifier;
            NotifyExpression = notifyExpr;
        }
    }

}
