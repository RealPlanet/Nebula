using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractExpressionStatement
        : AbstractStatement
    {
        public AbstractExpression Expression { get; }
        public override AbstractNodeType Type => AbstractNodeType.ExpressionStatement;
        public AbstractExpressionStatement(Node syntax, AbstractExpression expression)
            : base(syntax)
        {
            Expression = expression;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return Expression;
        }
    }
}
