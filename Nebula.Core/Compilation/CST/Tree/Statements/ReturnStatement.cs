using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed class ReturnStatement
        : Statement
    {
        public override NodeType Type => NodeType.ReturnStatement;

        public Token ReturnKeyword { get; }
        public Expression? Expression { get; }
        public Token Semicolon { get; }

        internal ReturnStatement(SourceCode sourceCode, Token returnKeyword, Expression? expression, Token semicolon)
            : base(sourceCode)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return ReturnKeyword;
            if (Expression != null)
            {
                yield return Expression;
            }

            yield return Semicolon;
        }
    }
}
