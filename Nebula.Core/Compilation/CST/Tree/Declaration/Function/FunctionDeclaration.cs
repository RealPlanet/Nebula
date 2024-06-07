using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Parsing.Statements;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class FunctionDeclaration
        : BaseFunctionDeclaration
    {
        public override NodeType Type => NodeType.FunctionDeclaration;

        public TokenSeparatedList<Token> Attributes { get; }
        public BlockStatement Body { get; }

        public FunctionDeclaration(
            SourceCode sourceCode,
            Token keyword,
            TypeClause returnType,
            Token name,
            Token openParenthesis,
            TokenSeparatedList<Parameter> parameters,
            Token closedParenthesis,
            TokenSeparatedList<Token> attributes,
            BlockStatement body)
            : base(sourceCode, keyword, returnType, name, openParenthesis, parameters, closedParenthesis)
        {
            Attributes = attributes;
            Body = body;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (Node n in base.GetChildren())
                yield return n;

            foreach (Node child in Attributes.GetWithSeparators())
                yield return child;

            yield return Body;
        }
    }
}
