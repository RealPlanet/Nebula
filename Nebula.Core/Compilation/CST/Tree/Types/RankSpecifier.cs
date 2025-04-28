using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class RankSpecifier
        : Node
    {
        public override NodeType Type => NodeType.RankSpecifier;

        public Token OpenSquareBracket { get; }
        public TokenSeparatedList<Token> Commas { get; }
        public Token ClosedSquareBracket { get; }

        public int Rank => (Commas.GetWithSeparators().Count - Commas.Count) + 1;

        public RankSpecifier(SourceCode sourceCode, Token openSquareBracket, TokenSeparatedList<Token> commas, Token closedSquareBracket)
            : base(sourceCode)
        {
            OpenSquareBracket = openSquareBracket;
            Commas = commas;
            ClosedSquareBracket = closedSquareBracket;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenSquareBracket;
            foreach (var c in Commas.GetWithSeparators())
            {
                yield return c;
            }

            yield return ClosedSquareBracket;
        }
    }
}
