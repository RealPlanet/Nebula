using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    /// <summary>
    /// An expression which takes another expression and converts it's result into a result which has
    /// a type corresponding to the one requested.
    /// </summary>
    public sealed class AbstractConversionExpression
        : AbstractExpression
    {
        /// <summary>
        /// The type which this conversion will result in.
        /// </summary>
        public override TypeSymbol ResultType { get; }

        /// <summary>
        /// The expression which will have it's result converted to the requested type.
        /// </summary>
        public AbstractExpression Expression { get; }
        public override AbstractNodeType Type => AbstractNodeType.ConversionExpression;

        public AbstractConversionExpression(Node syntax, TypeSymbol targetType, AbstractExpression expression)
            : base(syntax)
        {
            ResultType = targetType;
            Expression = expression;
        }
    }
}
