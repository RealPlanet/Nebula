using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed partial class ContinueStatement
        : Statement
    {
        public Token Keyword { get; }
        public Token Semicolon { get; }

        public override NodeType Type => NodeType.ContinueStatement;
        public ContinueStatement(SourceCode sourceCode, Token keyword, Token semicolon)
            : base(sourceCode)
        {
            Keyword = keyword;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return Semicolon;
        }
    }
}
