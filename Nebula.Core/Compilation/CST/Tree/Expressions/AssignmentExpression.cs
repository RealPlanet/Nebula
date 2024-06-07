using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed class AssignmentExpression
         : Expression
    {
        public override NodeType Type => NodeType.AssignmentExpression;
        public Expression Identifier { get; }
        public Token Operator { get; }
        public Expression RightExpr { get; }

        public AssignmentExpression(SourceCode sourceCode, Expression identifier, Token op, Expression rightExpr)
            : base(sourceCode)
        {
            Identifier = identifier;
            Operator = op;
            RightExpr = rightExpr;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return Operator;
            yield return RightExpr;
        }
    }
}
