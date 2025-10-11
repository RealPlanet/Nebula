using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public class AbstractVariableExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Variable.Type;
        public override AbstractNodeType Type => AbstractNodeType.VariableExpression;
        public VariableSymbol Variable { get; }
        public override AbstractConstant? ConstantValue => Variable.Constant;

        public AbstractVariableExpression(Node syntax, VariableSymbol variable)
            : base(syntax)
        {
            Variable = variable;
        }
    }
}
