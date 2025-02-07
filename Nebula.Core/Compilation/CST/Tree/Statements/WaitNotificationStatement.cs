using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed class WaitNotificationStatement 
        : Statement
    {
        public override NodeType Type => NodeType.WaitNotificationStatement;

        public NameExpression Identifier { get; }
        public Token Keyword { get; }
        public Expression Expression { get; }
        public Token Semicolon { get; }

        public WaitNotificationStatement(SourceCode code, NameExpression identifier, Token keyword, Expression expression, Token semicolon)
            : base(code)
        {
            Identifier = identifier;
            Keyword = keyword;
            Expression = expression;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return Keyword;
            yield return Expression;
            yield return Semicolon;
        }
    }
}
