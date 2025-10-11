using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed class WaitStatement
        : Statement
    {
        public override NodeType Type => NodeType.WaitStatement;

        public Token Keyword { get; }
        public Expression Time { get; }
        public Token Semicolon { get; }

        public WaitStatement(SourceCode sourceCode, Token keyword, Expression time, Token semicolon)
            : base(sourceCode)
        {
            Keyword = keyword;
            Time = time;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return Time;
            yield return Semicolon;
        }
    }
}
