using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements
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
