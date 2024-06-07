using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractCompoundAssignmentExpression
        : AbstractExpression
    {
        public AbstractCompoundAssignmentExpression(Node syntax, VariableSymbol variable, AbstractBinaryOperator op, AbstractExpression expression)
            : base(syntax)
        {
            Variable = variable;
            Operator = op;
            Expression = expression;
        }

        public override AbstractNodeType Type => AbstractNodeType.CompoundAssignmentExpression;
        public override TypeSymbol ResultType => Expression.ResultType;
        public VariableSymbol Variable { get; }
        public AbstractBinaryOperator Operator { get; }
        public AbstractExpression Expression { get; }
    }
}
