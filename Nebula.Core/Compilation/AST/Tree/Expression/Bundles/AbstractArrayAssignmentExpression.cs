using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractArrayAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.ArrayAssignmentExpression;

        public VariableSymbol ArrayVariable { get; }
        public AbstractExpression IndexExpression { get; }
        public AbstractExpression Expression { get; }

        public AbstractArrayAssignmentExpression(Node syntax, VariableSymbol arrayVariable, AbstractExpression indexExpression, AbstractExpression expression)
            : base(syntax)
        {
            ArrayVariable = arrayVariable;
            IndexExpression = indexExpression;
            Expression = expression;
        }
    }
}
