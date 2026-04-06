using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractNopStatement
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.NopStatement;

        public AbstractNopStatement(Node syntax)
            : base(syntax) { }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield break;
        }
    }
}
