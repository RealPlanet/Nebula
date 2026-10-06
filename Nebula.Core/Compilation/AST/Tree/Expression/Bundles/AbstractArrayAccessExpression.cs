using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractIndexExpression
        : AbstractExpression
    {
        public override AbstractNodeType Type => AbstractNodeType.IndexExpression;
        public AbstractExpression Target { get; }
        public AbstractExpression IndexToAccess { get; }

        public override TypeSymbol ResultType
        {
            get
            {
                if(Target.ResultType is ArrayTypeSymbol arrayType)
                {
                    return arrayType.ValueType;
                }

                return TypeSymbol.Error;
            }
        }
        public AbstractIndexExpression(Node syntax, AbstractExpression target, AbstractExpression indexToAccess)
            : base(syntax)
        {
            Target = target;
            IndexToAccess = indexToAccess;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return Target;
        }
    }
}
