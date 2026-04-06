using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractEndOnNotificationStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.EndOnNotificationStatement;

        public AbstractExpression BundleToEndOn { get; }
        public AbstractExpression NotifyExpression { get; }
        public AbstractEndOnNotificationStatement(Node syntax, AbstractExpression bundleNotifier, AbstractExpression notifyExpr)
            : base(syntax)
        {
            BundleToEndOn = bundleNotifier;
            NotifyExpression = notifyExpr;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return BundleToEndOn;
            yield return NotifyExpression;
        }
    }

}
