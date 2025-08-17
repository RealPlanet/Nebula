using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

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
    }
}
