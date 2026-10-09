using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractArrayInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => _internalAllocationType;
        public override AbstractNodeType Type => AbstractNodeType.ArrayInitializationExpression;

        private TypeSymbol _internalAllocationType;

        public AbstractArrayInitializationExpression(Node syntax)
            : base(syntax)
        {
            // Does nothing for now
            _internalAllocationType = TypeSymbol.BaseArray;
        }

        public void SetAllocationResult(TypeSymbol result)
        {
            _internalAllocationType = result;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield break;
        }
    }
}
