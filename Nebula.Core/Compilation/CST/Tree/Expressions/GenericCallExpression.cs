using Nebula.Commons.Collections;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public abstract class GenericCallExpression
        : Expression
    {
        public Token FunctionName { get; }
        public Token OpenParenthesis { get; }
        public TokenSeparatedList<Expression> Arguments { get; }
        public Token CloseParenthesis { get; }

        protected GenericCallExpression(SourceCode sourceCode, Token identifier, Token openParenthesis, TokenSeparatedList<Expression> args, Token closeParenthesis)
            : base(sourceCode)
        {
            FunctionName = identifier;
            OpenParenthesis = openParenthesis;
            Arguments = args;
            CloseParenthesis = closeParenthesis;
        }
    }
}
