using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed partial class DoWhileStatement
        : Statement
    {
        public Token DoKeyword { get; }
        public Statement Body { get; }
        public Token WhileKeyword { get; }
        public Expression Condition { get; }
        public Token Semicolon { get; }

        public override NodeType Type => NodeType.DoWhileStatement;
        public DoWhileStatement(SourceCode sourceCode, Token doKeyword, Statement body, Token whileKeyword, Expression condition, Token semicolon)
            : base(sourceCode)
        {
            DoKeyword = doKeyword;
            Body = body;
            WhileKeyword = whileKeyword;
            Condition = condition;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return DoKeyword;
            yield return Body;
            yield return WhileKeyword;
            yield return Condition;
            yield return Semicolon;
        }
    }
}
