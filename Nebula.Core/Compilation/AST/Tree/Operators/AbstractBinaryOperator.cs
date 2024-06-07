using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractBinaryOperator
    {
        public NodeType NodeType { get; }
        public AbstractdBinaryType BinaryType { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol ResultType { get; }

        private static readonly AbstractBinaryOperator[] Operators =
        {
            new(NodeType.PlusToken, AbstractdBinaryType.Addition, TypeSymbol.Int),
            new(NodeType.MinusToken, AbstractdBinaryType.Subtraction, TypeSymbol.Int),
            new(NodeType.SlashToken, AbstractdBinaryType.Division, TypeSymbol.Int),
            new(NodeType.StarToken, AbstractdBinaryType.Multiplication, TypeSymbol.Int),
            new(NodeType.ModuloToken, AbstractdBinaryType.Remainer, TypeSymbol.Int),

            new(NodeType.AmpersandToken, AbstractdBinaryType.BitwiseAnd, TypeSymbol.Int),
            new(NodeType.PipeToken, AbstractdBinaryType.BitwiseOr, TypeSymbol.Int),
            new(NodeType.HatToken, AbstractdBinaryType.BitwiseXor, TypeSymbol.Int),

            new(NodeType.DoubleEqualsToken, AbstractdBinaryType.Equals, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractdBinaryType.NotEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.LessToken, AbstractdBinaryType.LessThan, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.LessOrEqualsToken, AbstractdBinaryType.LessThanOrEqual, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.GreaterToken, AbstractdBinaryType.GreaterThan, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.GreaterOrEqualsToken, AbstractdBinaryType.GreaterThanOrEqual, TypeSymbol.Int, TypeSymbol.Bool),

            new(NodeType.AmpersandToken, AbstractdBinaryType.BitwiseAnd, TypeSymbol.Bool),
            new(NodeType.DoubleAmpersandToken, AbstractdBinaryType.LogicalAnd, TypeSymbol.Bool),
            new(NodeType.PipeToken, AbstractdBinaryType.BitwiseOr, TypeSymbol.Bool),
            new(NodeType.DoublePipeToken, AbstractdBinaryType.LogicalOr, TypeSymbol.Bool),
            new(NodeType.HatToken, AbstractdBinaryType.BitwiseXor, TypeSymbol.Bool),
            new(NodeType.DoubleEqualsToken, AbstractdBinaryType.Equals, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractdBinaryType.NotEquals, TypeSymbol.Bool),

            // Strings
            new(NodeType.PlusToken, AbstractdBinaryType.Addition, TypeSymbol.String),
            new(NodeType.DoubleEqualsToken, AbstractdBinaryType.Equals, TypeSymbol.String, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractdBinaryType.NotEquals, TypeSymbol.String,TypeSymbol.Bool),

           //new BoundBinaryOperator(NodeType.DoubleEqualsToken, BoundBinaryType.EQUALS, TypeSymbol.Any),
           //new BoundBinaryOperator(NodeType.BangEqualsToken, BoundBinaryType.NOT_EQUALS, TypeSymbol.Any),
        };

        private AbstractBinaryOperator(NodeType tokenType, AbstractdBinaryType binaryType, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            NodeType = tokenType;
            BinaryType = binaryType;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        private AbstractBinaryOperator(NodeType tokenType, AbstractdBinaryType binaryType, TypeSymbol type, TypeSymbol resultType)
            : this(tokenType, binaryType, type, type, resultType)
        {
        }

        private AbstractBinaryOperator(NodeType tokenType, AbstractdBinaryType boundType, TypeSymbol type)
            : this(tokenType, boundType, type, type, type)
        { }

        public static AbstractBinaryOperator? Bind(NodeType tokenType, TypeSymbol leftType, TypeSymbol rightLeft)
        {
            foreach (AbstractBinaryOperator op in Operators)
            {
                if (op.LeftType == leftType && op.NodeType == tokenType && op.RightType == rightLeft)
                {
                    return op;
                }
            }

            return null;
        }
    }

}
