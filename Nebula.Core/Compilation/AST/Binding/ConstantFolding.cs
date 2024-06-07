using Nebula.Core.Binding.Symbols;
using System;

namespace Nebula.Core.Binding
{
    public static class ConstantFolding
    {
        public static AbstractConstant? Fold(AbstractUnaryOperator op, AbstractExpression operand)
        {
            if (operand.ConstantValue is not null)
            {
                return op.UnaryType switch
                {
                    AbstractUnaryType.Negation => new(-(int)operand.ConstantValue.Value),
                    AbstractUnaryType.Identity => new((int)operand.ConstantValue.Value),
                    AbstractUnaryType.LogicalNegation => new(!(bool)operand.ConstantValue.Value),
                    AbstractUnaryType.OnesComplement => new(~(int)operand.ConstantValue.Value),
                    _ => throw new Exception($"Unexpected unary operator <{op.UnaryType}>"),
                };
            }

            return null;
        }

        public static AbstractConstant? Fold(AbstractExpression left, AbstractBinaryOperator op, AbstractExpression right)
        {
            AbstractConstant? leftConstant = left.ConstantValue;
            AbstractConstant? rightConstant = right.ConstantValue;

            #region AND/OR Short-circuit
            // Special cases that CAN be computed with partial values (AND / OR)
            // If either side of the binary operation is false then AND will result false
            if (op.BinaryType == AbstractdBinaryType.LogicalAnd)
            {
                if (leftConstant != null && !(bool)leftConstant.Value ||
                    rightConstant != null && !(bool)rightConstant.Value)
                {
                    return new AbstractConstant(false);
                }
            }

            // If either side of the binary operation is true then OR will result true
            if (op.BinaryType == AbstractdBinaryType.LogicalAnd)
            {
                if (leftConstant != null && (bool)leftConstant.Value ||
                    rightConstant != null && (bool)rightConstant.Value)
                {
                    return new AbstractConstant(true);
                }
            }
            #endregion

            if (leftConstant is null || rightConstant is null)
                return null;

            // We know both values are not null
            object? l = left.ConstantValue!.Value;
            object? r = right.ConstantValue!.Value;
            return op.BinaryType switch
            {
                AbstractdBinaryType.Addition => new(EvaluateAddition(op, l, r)),
                AbstractdBinaryType.Subtraction => new((int)l - (int)r),
                AbstractdBinaryType.Multiplication => new((int)l * (int)r),
                AbstractdBinaryType.Division => new((int)l / (int)r),

                AbstractdBinaryType.BitwiseAnd => new(EvaluateBitwiseAnd(op, l, r)),
                AbstractdBinaryType.BitwiseOr => new(EvaluateBitwiseOr(op, l, r)),
                AbstractdBinaryType.BitwiseXor => new(EvaluateBitwiseXOR(op, l, r)),

                AbstractdBinaryType.LogicalAnd => new((bool)l && (bool)r),
                AbstractdBinaryType.LogicalOr => new((bool)l || (bool)r),
                AbstractdBinaryType.Equals => new(Equals(l, r)),
                AbstractdBinaryType.NotEquals => new(!Equals(l, r)),
                AbstractdBinaryType.LessThan => new((int)l < (int)r),
                AbstractdBinaryType.LessThanOrEqual => new((int)l <= (int)r),
                AbstractdBinaryType.GreaterThan => new((int)l > (int)r),
                AbstractdBinaryType.GreaterThanOrEqual => new((int)l >= (int)r),
                _ => throw new Exception($"Unexpected binary operator <{op.BinaryType}>"),
            };
        }

        private static object EvaluateAddition(AbstractBinaryOperator op, object lValue, object rValue)
        {
            if (op.LeftType == TypeSymbol.Int)
                return (int)lValue + (int)rValue;
            //if (op.Type == TypeSymbol.String)
            return (string)lValue + (string)rValue;
        }

        private static object EvaluateBitwiseOr(AbstractBinaryOperator op, object lValue, object rValue)
        {
            if (op.LeftType == TypeSymbol.Int)
                return (int)lValue | (int)rValue;

            return (bool)lValue || (bool)rValue;
        }

        private static object EvaluateBitwiseXOR(AbstractBinaryOperator op, object lValue, object rValue)
        {
            if (op.LeftType == TypeSymbol.Int)
                return (int)lValue ^ (int)rValue;

            return (bool)lValue ^ (bool)rValue;
        }

        private static object EvaluateBitwiseAnd(AbstractBinaryOperator op, object lValue, object rValue)
        {
            if (op.LeftType == TypeSymbol.Int)
                return (int)lValue & (int)rValue;

            return (bool)lValue && (bool)rValue;
        }
    }
}
