using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public class ObjectCallExpression
        : CallExpression
    {
        public override NodeType Type => NodeType.ObjectCallExpression;
        public Token ObjectIdentifier { get; }
        public Token DotToken { get; }

        public ObjectCallExpression(SourceCode sourceCode, Token objectIdentifier, Token dotToken, Token identifier, Token openParenthesis, TokenSeparatedList<Expression> args, Token closeParenthesis)
            : base(sourceCode, null, null, null, identifier, openParenthesis, args, closeParenthesis)
        {
            ObjectIdentifier = objectIdentifier;
            DotToken = dotToken;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return ObjectIdentifier;
            yield return DotToken;
            foreach (Node c in base.GetChildren())
            {
                yield return c;
            }
        }
    }
}
