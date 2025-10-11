using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed partial class WhileStatement
        : Statement
    {
        public Token WhileKeyword { get; }
        public Token OpenParenthesis { get; }
        public Expression Condition { get; }
        public Token CloseParenthesis { get; }
        public Statement Body { get; }
        public override NodeType Type => NodeType.WhileStatement;

        internal WhileStatement(SourceCode sourceCode, Token whileKeyword, Token openParenthesis, Expression condition, Token closeParenthesis, Statement body)
            : base(sourceCode)
        {
            WhileKeyword = whileKeyword;
            OpenParenthesis = openParenthesis;
            Condition = condition;
            CloseParenthesis = closeParenthesis;
            Body = body;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return WhileKeyword;
            yield return Condition;
            yield return Body;
        }
    }
}
