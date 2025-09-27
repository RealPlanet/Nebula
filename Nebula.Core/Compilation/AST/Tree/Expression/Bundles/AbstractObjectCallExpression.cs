using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.AST.Tree.Expression.Bundles
{
    public sealed class AbstractObjectCallExpression
        : AbstractCallExpression
    {
        public override AbstractNodeType Type => AbstractNodeType.ObjectCallExpression;

        public VariableSymbol Variable { get; }

        public AbstractObjectCallExpression(Node syntax, VariableSymbol variable, FunctionSymbol function, ImmutableArray<AbstractExpression> arguments)
            : base(syntax, false, string.Empty, function, arguments)
        {
            Variable = variable;
        }
    }
}
