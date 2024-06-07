using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public abstract class BaseFunctionDeclaration
        : Statement
    {
        public Token Keyword { get; }
        public TypeClause ReturnType { get; }
        public Token Name { get; }
        public Token OpenParenthesis { get; }
        public TokenSeparatedList<Parameter> Parameters { get; }
        public Token ClosedParenthesis { get; }

        private protected BaseFunctionDeclaration(
            SourceCode syntaxTree,
            Token keyword,
            TypeClause returnType,
            Token name,
            Token openParenthesis,
            TokenSeparatedList<Parameter> parameters,
            Token closedParenthesis)
            : base(syntaxTree)
        {
            Keyword = keyword;
            ReturnType = returnType;
            Name = name;
            OpenParenthesis = openParenthesis;
            Parameters = parameters;
            ClosedParenthesis = closedParenthesis;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return ReturnType;
            yield return Name;
            yield return OpenParenthesis;
            foreach (Node child in Parameters.GetWithSeparators())
                yield return child;

            yield return ClosedParenthesis;
        }
    }
}
