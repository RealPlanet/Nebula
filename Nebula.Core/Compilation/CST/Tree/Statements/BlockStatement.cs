using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Core.Parsing.Statements
{
    public sealed class BlockStatement
        : Statement
    {
        public override NodeType Type => NodeType.BlockStatement;

        public Token OpenBracket { get; }
        public ImmutableArray<Statement> Statements { get; }
        public Token CloseBracket { get; }

        public BlockStatement(SourceCode sourceCode, Token openBracket, ImmutableArray<Statement> statements, Token closeBracket)
            : base(sourceCode)
        {
            OpenBracket = openBracket;
            Statements = statements;
            CloseBracket = closeBracket;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenBracket;
            foreach (Statement statement in Statements)
                yield return statement;
            yield return CloseBracket;
        }
    }
}
