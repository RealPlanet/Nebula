using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    /// <summary>
    /// Rapresents an Expression within the Bound Syntax Tree
    /// </summary>
    public abstract class AbstractExpression
        : AbstractNode
    {
        /// <summary>
        /// The type of the result of this expression
        /// </summary>
        public abstract TypeSymbol ResultType { get; }

        /// <summary>
        /// A constant representation of this expression if this expression could be lowered into a one. By default its null.
        /// </summary>
        public virtual AbstractConstant? ConstantValue => null;

        protected AbstractExpression(Node syntax)
            : base(syntax) { }
    }
}
