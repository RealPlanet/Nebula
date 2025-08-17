using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

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
    }
}
