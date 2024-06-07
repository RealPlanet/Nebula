using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed partial class IfStatement
        : Statement
    {
        public override NodeType Type => NodeType.IfStatement;
        public Token IfKeyword { get; }
        public Token OpenParenthesis { get; }
        public Expression Condition { get; }
        public Token CloseParenthesis { get; }
        public Statement ThenStatement { get; }
        public ElseClauseStatement? ElseClause { get; }

        internal IfStatement(SourceCode sourceCode, Token ifKeyword, Token openParenthesis, Expression condition, Token closeParenthesis, Statement thenStatement, ElseClauseStatement? elseClause)
            : base(sourceCode)
        {
            IfKeyword = ifKeyword;
            OpenParenthesis = openParenthesis;
            Condition = condition;
            CloseParenthesis = closeParenthesis;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return IfKeyword;
            yield return OpenParenthesis;
            yield return Condition;
            yield return CloseParenthesis;
            yield return ThenStatement;

            if (ElseClause != null)
                yield return ThenStatement;
        }
    }
}
