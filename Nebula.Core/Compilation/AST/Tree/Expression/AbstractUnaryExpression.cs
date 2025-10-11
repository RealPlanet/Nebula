using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Binding;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Operators;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractUnaryExpression
        : AbstractExpression
    {
        public override AbstractNodeType Type => AbstractNodeType.UnaryExpression;
        public override TypeSymbol ResultType => Operator.ResultType;
        public AbstractUnaryOperator Operator { get; }
        public AbstractExpression Operand { get; }
        public override AbstractConstant? ConstantValue { get; }

        public AbstractUnaryExpression(Node syntax, AbstractUnaryOperator op, AbstractExpression operand)
            : base(syntax)
        {
            Operator = op;
            Operand = operand;
            ConstantValue = ConstantFolding.Fold(op, operand);
        }
    }
}
