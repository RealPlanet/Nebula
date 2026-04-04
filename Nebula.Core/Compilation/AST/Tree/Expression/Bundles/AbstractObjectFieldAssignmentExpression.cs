using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractObjectFieldAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.ObjectFieldAssignmentExpression;

        public AbstractExpression TargetExpression { get; }
        public AbstractBundleField Field { get; }
        public AbstractExpression Expression { get; }

        public AbstractObjectFieldAssignmentExpression(Node syntax, AbstractExpression targetExpression, AbstractBundleField field, AbstractExpression expression)
            : base(syntax)
        {
            TargetExpression = targetExpression;
            Field = field;
            Expression = expression;
        }
    }
}
