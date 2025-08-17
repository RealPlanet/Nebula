using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed partial class BreakStatement
        : Statement
    {
        public Token Keyword { get; }
        public Token Semicolon { get; }
        public override NodeType Type => NodeType.BreakStatement;

        internal BreakStatement(SourceCode sourceCode, Token keyword, Token semicolon)
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
