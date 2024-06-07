using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class LiteralExpression
        : Expression
    {
        public override NodeType Type => NodeType.LiteralExpression;

        public Token Literal { get; }

        public object Value { get; }

        internal LiteralExpression(SourceCode soureCode, Token literalToken, object value)
            : base(soureCode)
        {
            Literal = literalToken;
            Value = value;
        }

        internal LiteralExpression(SourceCode soureCode, Token literalToken)
            : this(soureCode, literalToken, literalToken.Value!)
        {
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Literal;
        }
    }
}
