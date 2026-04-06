using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow
{
    public sealed class AbstractReturnStatement
        : AbstractStatement
    {
        public AbstractExpression? Expression { get; }
        public override AbstractNodeType Type => AbstractNodeType.ReturnStatement;
        public AbstractReturnStatement(Node syntax, AbstractExpression? expression)
            : base(syntax)
        {
            Expression = expression;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            if (Expression != null)
                yield return Expression;
        }
    }
}
