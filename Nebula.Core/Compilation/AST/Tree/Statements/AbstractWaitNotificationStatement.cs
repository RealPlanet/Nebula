using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractWaitNotificationStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WaitNotificationStatement;

        public AbstractExpression ObjectToWait { get; }
        public AbstractExpression NotifyExpression { get; }
        public AbstractWaitNotificationStatement(Node syntax, AbstractExpression bundleNotifier, AbstractExpression notifyExpr)
            : base(syntax)
        {
            ObjectToWait = bundleNotifier;
            NotifyExpression = notifyExpr;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return ObjectToWait;
            yield return NotifyExpression;
        }
    }
}
