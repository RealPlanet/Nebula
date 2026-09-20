using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Types
{
    public class TypeClause
        : BaseTypeClause
    {
        public override NodeType Type => NodeType.TypeClause;
        public Token Identifier { get; }

        public TypeClause(SourceCode sourceCode, Token identifier)
            : base(sourceCode)
        {
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
        }

        public override string ToString() => Identifier.Text;
    }
}
