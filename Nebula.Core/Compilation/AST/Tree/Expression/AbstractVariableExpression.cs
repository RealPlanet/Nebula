using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public class AbstractVariableExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => ArrayVariable.Type;
        public override AbstractNodeType Type => AbstractNodeType.VariableExpression;
        public VariableSymbol ArrayVariable { get; }
        public override AbstractConstant? ConstantValue => ArrayVariable.Constant;

        public AbstractVariableExpression(Node syntax, VariableSymbol variable)
            : base(syntax)
        {
            ArrayVariable = variable;
        }
    }
}
