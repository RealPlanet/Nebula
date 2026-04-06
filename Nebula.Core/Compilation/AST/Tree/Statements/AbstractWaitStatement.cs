using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractWaitStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WaitStatement;

        public AbstractExpression TimeExpression { get; }

        public AbstractWaitStatement(Node syntax, AbstractExpression time)
            : base(syntax)
        {
            TimeExpression = time;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return TimeExpression;
        }
    }
}
