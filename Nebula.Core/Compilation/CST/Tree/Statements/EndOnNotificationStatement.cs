using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed class EndOnNotificationStatement
        : Statement
    {
        public override NodeType Type => NodeType.EndOnNotificationKeyword;

        public NameExpression Identifier { get; }
        public Token Keyword { get; }
        public Expression Expression { get; }
        public Token Semicolon { get; }

        public EndOnNotificationStatement(SourceCode code, NameExpression identifier, Token keyword, Expression expression, Token semicolon)
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
