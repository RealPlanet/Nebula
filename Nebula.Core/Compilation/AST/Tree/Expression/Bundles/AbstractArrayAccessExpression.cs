using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
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
