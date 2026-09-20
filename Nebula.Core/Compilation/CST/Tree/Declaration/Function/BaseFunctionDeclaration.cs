using Nebula.Commons.Collections;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Core.Compilation.CST.Tree.Declaration.Function
{
    public abstract class BaseFunctionDeclaration
        : Statement
    {
        public Token Keyword { get; }
        public BaseTypeClause ReturnType { get; }
        public Token Name { get; }
        public Token OpenParenthesis { get; }
        public TokenSeparatedList<Parameter> Parameters { get; }
        public Token ClosedParenthesis { get; }

        private protected BaseFunctionDeclaration(
            SourceCode syntaxTree,
            Token keyword,
            BaseTypeClause returnType,
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

        public TextLocation SignatureLocation
        {
            get
            {
                TextSpan first = GetChildren().First().Span;
                TextSpan last = GetChildren().Last().Span;
                var span = TextSpan.FromBounds(first.Start, last.End);
                return new TextLocation(SourceCode, span);
            }
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            foreach (Node child in ReturnType.GetChildren())
            {
                yield return child;
            }

            yield return Name;
            yield return OpenParenthesis;
            foreach (Node child in Parameters.GetWithSeparators())
            {
                yield return child;
            }

            yield return ClosedParenthesis;
        }
    }
}
