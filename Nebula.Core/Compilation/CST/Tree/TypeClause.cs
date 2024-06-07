using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{

    public sealed class TypeClause
        : Node
    {
        public override NodeType Type => NodeType.TypeClause;
        public Token? Namespace { get; }
        public Token? DoubleColonToken { get; }
        public Token Identifier { get; }

        public TypeClause(SourceCode sourceCode, Token? @namespace, Token? doubleColonToken, Token identifier)
            : base(sourceCode)
        {
            Namespace = @namespace;
            DoubleColonToken = doubleColonToken;
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            if (Namespace != null)
            {
                yield return Namespace;
                yield return DoubleColonToken!;
            }

            yield return Identifier;
        }
    }
}
