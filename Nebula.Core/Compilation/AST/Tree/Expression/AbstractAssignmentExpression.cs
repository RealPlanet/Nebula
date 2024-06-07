using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    /// <summary>
    /// An assignment expression withing Bound Syntax Tree.
    /// </summary>
    public sealed class AbstractAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.AssignmentExpression;

        /// <summary>
        /// The variable to which assign the result of the Expression.
        /// </summary>
        public VariableSymbol Variable { get; }

        /// <summary>
        /// The R-Value of this assignment which can be any type of expresion which returns a result.
        /// </summary>
        public AbstractExpression Expression { get; }
        public AbstractAssignmentExpression(Node syntax, VariableSymbol variable, AbstractExpression expression)
            : base(syntax)
        {
            Variable = variable;
            Expression = expression;
        }
    }
}
