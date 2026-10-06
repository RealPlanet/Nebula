using Nebula.Commons.Collections;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{

    public class CallExpression
        : GenericCallExpression
    {
        public override NodeType Type => NodeType.CallExpression;

        public Token? AsyncCall { get; }
        public Token? Namespace { get; }
        public Token? DoubleColon { get; }

        public bool IsAsyncCall => AsyncCall != null;

        public CallExpression(SourceCode sourceCode,
                              Token? asyncCall,
                              Token? @namespace,
                              Token? doubleColon,
                              Token functionName,
                              Token openParenthesis,
                              TokenSeparatedList<Expression> args,
                              Token closeParenthesis)
            : base(sourceCode, functionName, openParenthesis, args, closeParenthesis)
        {
            AsyncCall = asyncCall;
            Namespace = @namespace;
            DoubleColon = doubleColon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            if (AsyncCall != null)
            {
                yield return AsyncCall;
            }

            if (DoubleColon != null)
            {
                yield return DoubleColon;
            }

            if (Namespace != null)
            {
                yield return Namespace;
            }

            yield return FunctionName;
            yield return OpenParenthesis;

            foreach (Node argument in Arguments.GetWithSeparators())
            {
                yield return argument;
            }

            yield return CloseParenthesis;
        }
    }
}
