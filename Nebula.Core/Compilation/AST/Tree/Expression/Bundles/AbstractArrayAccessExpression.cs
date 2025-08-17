using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractArrayAccessExpression
        : AbstractVariableExpression
    {
        public AbstractExpression IndexExpression { get; }
        public override TypeSymbol ResultType => ((ArrayTypeSymbol)Variable.Type).ValueType;
        public AbstractArrayAccessExpression(Node syntax, VariableSymbol arrayVariable, AbstractExpression indexToAccess)
            : base(syntax, arrayVariable)
        {
            IndexExpression = indexToAccess;
        }
    }
}
