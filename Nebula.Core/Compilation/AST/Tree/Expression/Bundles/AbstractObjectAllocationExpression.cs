using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractObjectAllocationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Target.ResultType;

        public override AbstractNodeType Type => AbstractNodeType.ObjectAllocationExpression;

        public AbstractExpression Target { get; }

        public AbstractInitializationExpression Initializer { get; }

        public AbstractObjectAllocationExpression(Node node, AbstractExpression target, AbstractInitializationExpression initializer)
            : base(node)
        {
            Target = target;
            Initializer = initializer;
        }
    }
}
