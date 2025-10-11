using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements
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
