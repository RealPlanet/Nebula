using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => _internalAllocationType;
        public override AbstractNodeType Type => AbstractNodeType.InitializationExpression;

        private TypeSymbol _internalAllocationType;

        public AbstractInitializationExpression(Node syntax)
            : base(syntax)
        {
            // Does nothing for now
            _internalAllocationType = TypeSymbol.BaseObject;
        }

        public void SetAllocationResult(TypeSymbol result)
        {
            _internalAllocationType = result;
        }
    }
}
