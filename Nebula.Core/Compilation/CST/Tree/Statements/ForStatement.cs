using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed class ForStatement
        : Statement
    {
        public override NodeType Type => NodeType.ForStatement;

        public Token Keyword { get; }
        public Token OpenParenthesis { get; }
        public Statement InitStatement { get; }
        public Expression? Condition { get; }
        public Token SecondSemicolon { get; }
        public Expression? Expression { get; }
        public Token CloseParenthesis { get; }
        public Statement Body { get; }

        public ForStatement(SourceCode sourceCode, Token keyword, Token openParenthesis, Statement initStatement, Expression? condition, Token secondSemicolon, Expression? expressionStatement, Token closeParenthesis, Statement forBody)
        : base(sourceCode)
        {
            Keyword = keyword;
            OpenParenthesis = openParenthesis;
            InitStatement = initStatement;
            Condition = condition;
            SecondSemicolon = secondSemicolon;
            Expression = expressionStatement;
            CloseParenthesis = closeParenthesis;
            Body = forBody;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return OpenParenthesis;
            if (InitStatement is not null)
            {
                yield return InitStatement;
            }

            if (Condition is not null)
            {
                yield return Condition;
            }

            yield return SecondSemicolon;
            if (Expression is not null)
            {
                yield return Expression;
            }

            yield return CloseParenthesis;
            yield return Body;
        }
    }
}
