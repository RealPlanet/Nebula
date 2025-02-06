using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractBinaryOperator
    {
        public NodeType NodeType { get; }
        public AbstractBinaryType BinaryType { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol ResultType { get; }

        private static readonly AbstractBinaryOperator[] Operators =
        {
           // Int Math
           new(NodeType.PlusToken, AbstractBinaryType.Addition, TypeSymbol.Int),
           new(NodeType.MinusToken, AbstractBinaryType.Subtraction, TypeSymbol.Int),
           new(NodeType.SlashToken, AbstractBinaryType.Division, TypeSymbol.Int),
           new(NodeType.StarToken, AbstractBinaryType.Multiplication, TypeSymbol.Int),
           new(NodeType.ModuloToken, AbstractBinaryType.Remainer, TypeSymbol.Int),
           
            // Float Math
            new(NodeType.PlusToken, AbstractBinaryType.Addition, TypeSymbol.Float),
            new(NodeType.MinusToken, AbstractBinaryType.Subtraction, TypeSymbol.Float),
            new(NodeType.SlashToken, AbstractBinaryType.Division, TypeSymbol.Float),
            new(NodeType.StarToken, AbstractBinaryType.Multiplication, TypeSymbol.Float),
            new(NodeType.ModuloToken, AbstractBinaryType.Remainer, TypeSymbol.Float),

            // Int And FloatMath
            new(NodeType.PlusToken, AbstractBinaryType.Addition, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(NodeType.MinusToken, AbstractBinaryType.Subtraction, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(NodeType.SlashToken, AbstractBinaryType.Division, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),
            new(NodeType.StarToken, AbstractBinaryType.Multiplication, TypeSymbol.Float, TypeSymbol.Int, TypeSymbol.Float),

            new(NodeType.PlusToken, AbstractBinaryType.Addition, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(NodeType.MinusToken, AbstractBinaryType.Subtraction, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(NodeType.SlashToken, AbstractBinaryType.Division, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),
            new(NodeType.StarToken, AbstractBinaryType.Multiplication, TypeSymbol.Int, TypeSymbol.Float, TypeSymbol.Float),


            new(NodeType.AmpersandToken, AbstractBinaryType.BitwiseAnd, TypeSymbol.Int),
            new(NodeType.PipeToken, AbstractBinaryType.BitwiseOr, TypeSymbol.Int),
            new(NodeType.HatToken, AbstractBinaryType.BitwiseXor, TypeSymbol.Int),

            new(NodeType.DoubleEqualsToken, AbstractBinaryType.Equals, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractBinaryType.NotEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.LessToken, AbstractBinaryType.LessThan, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.LessOrEqualsToken, AbstractBinaryType.LessThanOrEqual, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.GreaterToken, AbstractBinaryType.GreaterThan, TypeSymbol.Int, TypeSymbol.Bool),
            new(NodeType.GreaterOrEqualsToken, AbstractBinaryType.GreaterThanOrEqual, TypeSymbol.Int, TypeSymbol.Bool),

            new(NodeType.AmpersandToken, AbstractBinaryType.BitwiseAnd, TypeSymbol.Bool),
            new(NodeType.DoubleAmpersandToken, AbstractBinaryType.LogicalAnd, TypeSymbol.Bool),
            new(NodeType.PipeToken, AbstractBinaryType.BitwiseOr, TypeSymbol.Bool),
            new(NodeType.DoublePipeToken, AbstractBinaryType.LogicalOr, TypeSymbol.Bool),
            new(NodeType.HatToken, AbstractBinaryType.BitwiseXor, TypeSymbol.Bool),
            new(NodeType.DoubleEqualsToken, AbstractBinaryType.Equals, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractBinaryType.NotEquals, TypeSymbol.Bool),
           
            // Strings
            new(NodeType.PlusToken, AbstractBinaryType.Addition, TypeSymbol.String),
            new(NodeType.DoubleEqualsToken, AbstractBinaryType.Equals, TypeSymbol.String, TypeSymbol.Bool),
            new(NodeType.BangEqualsToken, AbstractBinaryType.NotEquals, TypeSymbol.String,TypeSymbol.Bool),
           
           //new BoundBinaryOperator(NodeType.DoubleEqualsToken, BoundBinaryType.EQUALS, TypeSymbol.Any),
           //new BoundBinaryOperator(NodeType.BangEqualsToken, BoundBinaryType.NOT_EQUALS, TypeSymbol.Any),
        };

        private AbstractBinaryOperator(NodeType tokenType, AbstractBinaryType binaryType, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            NodeType = tokenType;
            BinaryType = binaryType;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        private AbstractBinaryOperator(NodeType tokenType, AbstractBinaryType binaryType, TypeSymbol type, TypeSymbol resultType)
            : this(tokenType, binaryType, type, type, resultType)
        {
        }

        private AbstractBinaryOperator(NodeType tokenType, AbstractBinaryType boundType, TypeSymbol type)
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
