using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    /// <summary>
    /// Defines the type of unary operations by definiing the node type, the operation type and result typee
    /// </summary>
    public sealed class AbstractUnaryOperator
    {
        public NodeType NodeType { get; }
        public AbstractUnaryType UnaryType { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol ResultType { get; }

        private static readonly AbstractUnaryOperator[] Operators =
        {
            new(NodeType.BangToken, AbstractUnaryType.LogicalNegation, TypeSymbol.Bool),
            new(NodeType.PlusToken, AbstractUnaryType.Identity, TypeSymbol.Int),
            new(NodeType.MinusToken, AbstractUnaryType.Negation, TypeSymbol.Int),
            new(NodeType.TildeToken, AbstractUnaryType.OnesComplement, TypeSymbol.Int),
        };

        private AbstractUnaryOperator(NodeType tokenType, AbstractUnaryType unaryType, TypeSymbol operandType, TypeSymbol resultType)
        {
            NodeType = tokenType;
            UnaryType = unaryType;
            OperandType = operandType;
            ResultType = resultType;
        }

        private AbstractUnaryOperator(NodeType tokenType, AbstractUnaryType unaryType, TypeSymbol operandType)
            : this(tokenType, unaryType, operandType, operandType) { }

        /// <summary>
        /// Given the operator and type that it is operating on return a valid unary operator, if null no operation is allowed
        /// </summary>
        public static AbstractUnaryOperator? Bind(NodeType tokenType, TypeSymbol operandType)
        {
            foreach (AbstractUnaryOperator? op in Operators)
            {
                if (op.OperandType == operandType && op.NodeType == tokenType)
                {
                    return op;
                }
            }

            return null;
        }
    }

}
