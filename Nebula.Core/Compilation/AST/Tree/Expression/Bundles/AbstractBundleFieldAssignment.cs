using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
{
    public sealed class AbstractBundleFieldAssignment
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Expression.ResultType;
        public override AbstractNodeType Type => AbstractNodeType.BundleFieldAssignmentExpression;

        public VariableSymbol BundleVariable { get; }
        public AbstractBundleField FieldToAssign { get; }
        public AbstractExpression Expression { get; }

        public AbstractBundleFieldAssignment(Node syntax, VariableSymbol bundleVariable, AbstractBundleField field, AbstractExpression expression)
            : base(syntax)
        {
            BundleVariable = bundleVariable;
            FieldToAssign = field;
            Expression = expression;
        }
    }
}
