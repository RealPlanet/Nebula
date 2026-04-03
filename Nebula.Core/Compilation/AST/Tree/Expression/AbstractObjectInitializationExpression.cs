using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractObjectInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => _internalAllocationType;

        public override AbstractNodeType Type => AbstractNodeType.ObjectInitializationExpression;

        public ImmutableArray<AbstractObjectFieldInitializationExpression> FieldExpressions { get; }

        private TypeSymbol _internalAllocationType;

        public AbstractObjectInitializationExpression(Node syntax, ImmutableArray<AbstractObjectFieldInitializationExpression> abstractExpressions)
            : base(syntax)
        {
            // Does nothing for now
            _internalAllocationType = TypeSymbol.BaseObject;
            FieldExpressions = abstractExpressions;
        }

        public void SetAllocationResult(TypeSymbol result)
        {
            _internalAllocationType = result;
        }
    }
}
