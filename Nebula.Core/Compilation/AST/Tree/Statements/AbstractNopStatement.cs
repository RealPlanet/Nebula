using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractNopStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.NopStatement;

        public AbstractNopStatement(Node syntax)
            : base(syntax) { }
    }
}
