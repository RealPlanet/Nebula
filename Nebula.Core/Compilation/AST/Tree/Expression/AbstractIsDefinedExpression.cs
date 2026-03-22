using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractIsDefinedExpression
        : AbstractExpression
    {
        public AbstractExpression Expression { get; }

        public override TypeSymbol ResultType => TypeSymbol.Bool;

        public override AbstractNodeType Type => AbstractNodeType.IsDefinedExpression;

        public AbstractIsDefinedExpression(Node syntax, AbstractExpression expression)
            : base(syntax)
        {
            Expression = expression;
        }
    }
}
