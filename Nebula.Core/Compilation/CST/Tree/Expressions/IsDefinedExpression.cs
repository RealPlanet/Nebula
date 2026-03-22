using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class IsDefinedExpression
        : Expression
    {
        public override NodeType Type => NodeType.IsDefinedExpression;

        public Token Keyword { get; }
        public Token OpenParenthesis { get; }
        public Expression Expression { get; }
        public Token ClosedParenthesis { get; }

        public IsDefinedExpression(SourceCode sourceCode, Token keyword, Token openParenthesis, Expression expr, Token closedParenthesis)
            : base(sourceCode)
        {
            Keyword = keyword;
            OpenParenthesis = openParenthesis;
            Expression = expr;
            ClosedParenthesis = closedParenthesis;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return OpenParenthesis;
            yield return Expression;
            yield return ClosedParenthesis;
        }
    }
}
