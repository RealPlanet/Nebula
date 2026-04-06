using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractNotifyStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.NotifyStatement;

        public AbstractExpression ObjectToNotifyFrom { get; }
        public AbstractExpression NotifyExpression { get; }

        public AbstractNotifyStatement(Node syntax, AbstractExpression bundleNotifier, AbstractExpression notifyExpr)
            : base(syntax)
        {
            ObjectToNotifyFrom = bundleNotifier;
            NotifyExpression = notifyExpr;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return ObjectToNotifyFrom;
            yield return NotifyExpression;
        }
    }

}
