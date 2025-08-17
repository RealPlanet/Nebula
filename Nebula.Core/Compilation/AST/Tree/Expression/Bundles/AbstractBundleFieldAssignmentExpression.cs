using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractBundleFieldAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.BundleFieldAssignmentExpression;

        public VariableSymbol BundleVariable { get; }
        public AbstractBundleField FieldToAssign { get; }
        public AbstractExpression Expression { get; }

        public AbstractBundleFieldAssignmentExpression(Node syntax, VariableSymbol bundleVariable, AbstractBundleField field, AbstractExpression expression)
            : base(syntax)
        {
            BundleVariable = bundleVariable;
            FieldToAssign = field;
            Expression = expression;
        }
    }

    public sealed class AbstractArrayAssignmentExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.ArrayAssignmentExpression;

        public VariableSymbol ArrayVariable { get; }
        public AbstractExpression IndexExpression { get; }
        public AbstractExpression Expression { get; }

        public AbstractArrayAssignmentExpression(Node syntax, VariableSymbol arrayVariable, AbstractExpression indexExpression, AbstractExpression expression)
            : base(syntax)
        {
            ArrayVariable = arrayVariable;
            IndexExpression = indexExpression;
            Expression = expression;
        }
    }
}
