using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Base
{
    public sealed partial class ExpressionStatement
         : Statement
    {
        public override NodeType Type => NodeType.ExpressionStatement;

        public Expression Expression { get; }
        public Token Semicolon { get; }

        public ExpressionStatement(SourceCode sourceCode, Expression expr, Token semicolon)
            : base(sourceCode)
        {
            Expression = expr;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Expression;
            yield return Semicolon;
        }
    }
}
