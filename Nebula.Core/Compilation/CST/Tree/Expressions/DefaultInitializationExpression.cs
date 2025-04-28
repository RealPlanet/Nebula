using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed class DefaultInitializationExpression
        : Expression
    {
        public DefaultInitializationExpression(SourceCode sourceCode, Token openSquareBracket, Token closedSquareBracket)
            : base(sourceCode)
        {
            OpenSquareBracket = openSquareBracket;
            ClosedSquareBracket = closedSquareBracket;
        }

        public override NodeType Type => NodeType.DefaultInitializationExpression;

        public Token OpenSquareBracket { get; }
        public Token ClosedSquareBracket { get; }

        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenSquareBracket;
            yield return ClosedSquareBracket;
        }
    }
}
