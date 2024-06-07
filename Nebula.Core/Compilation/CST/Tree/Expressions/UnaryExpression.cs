using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Expressions
{
    public sealed class UnaryExpression
        : Expression
    {
        public UnaryExpression(SourceCode sourceCode, Token op, Expression operand)
            : base(sourceCode)
        {
            Operator = op;
            Operand = operand;
        }

        public override NodeType Type => NodeType.UnaryExpression;

        public Token Operator { get; }
        public Expression Operand { get; }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Operator;
            yield return Operand;
        }
    }
}
