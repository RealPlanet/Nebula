using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class ParenthesizedExpression
        : Expression
    {
        public override NodeType Type => NodeType.ParenthesizedExpression;
        public Token OpenParenthesisToken { get; }
        public Expression Expression { get; }
        public Token ClosedParenthesisToken { get; }

        internal ParenthesizedExpression(SourceCode sourceCode, Token openParenthesisToken, Expression expression, Token closedParenthesisToken)
            : base(sourceCode)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            ClosedParenthesisToken = closedParenthesisToken;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return ClosedParenthesisToken;
        }
    }
}
