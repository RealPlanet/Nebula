using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed partial class ElseClauseStatement
        : Node
    {
        public override NodeType Type => NodeType.ElseClause;
        public Token ElseKeyword { get; }
        public Statement ElseStatement { get; }

        public ElseClauseStatement(SourceCode sourceCode, Token elseKeyword, Statement elseStatement)
            : base(sourceCode)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return ElseKeyword;
            yield return ElseStatement;
        }
    }
}
