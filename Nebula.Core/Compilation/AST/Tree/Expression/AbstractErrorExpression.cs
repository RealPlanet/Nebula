using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    /// <summary>
    /// An error expression only used during the binding process.
    /// </summary>
    public sealed class AbstractErrorExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => TypeSymbol.Error;
        public override AbstractNodeType Type => AbstractNodeType.ErrorExpression;
        public AbstractErrorExpression(Node syntax)
            : base(syntax) { }
    }
}
