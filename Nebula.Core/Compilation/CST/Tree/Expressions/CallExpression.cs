using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{

    public class CallExpression
        : Expression
    {
        public override NodeType Type => NodeType.CallExpression;

        public Token? AsyncCall { get; }
        public Token? Namespace { get; }
        public Token? DoubleColon { get; }
        public Token Identifier { get; }
        public Token OpenParenthesis { get; }
        public TokenSeparatedList<Expression> Arguments { get; }
        public Token CloseParenthesis { get; }

        public bool IsAsyncCall => AsyncCall != null;

        public CallExpression(SourceCode sourceCode, Token? asyncCall, Token? @namespace, Token? doubleColon, Token identifier, Token openParenthesis, TokenSeparatedList<Expression> args, Token closeParenthesis)
            : base(sourceCode)
        {
            AsyncCall = asyncCall;
            Namespace = @namespace;
            DoubleColon = doubleColon;
            Identifier = identifier;
            OpenParenthesis = openParenthesis;
            Arguments = args;
            CloseParenthesis = closeParenthesis;
        }

        public override IEnumerable<Node> GetChildren()
        {
            if (AsyncCall != null)
                yield return AsyncCall;

            if (DoubleColon != null)
                yield return DoubleColon;

            if (Namespace != null)
                yield return Namespace;

            yield return Identifier;
            yield return OpenParenthesis;

            foreach (Node argument in Arguments.GetWithSeparators())
                yield return argument;

            yield return CloseParenthesis;
        }
    }
}
