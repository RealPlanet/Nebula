using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType { get; }
        public override AbstractNodeType Type => AbstractNodeType.InitializationExpression;

        public AbstractInitializationExpression(Node syntax)
            : base(syntax)
        {
            // Does nothing for now
            ResultType = TypeSymbol.BaseObject;
        }
    }
}
