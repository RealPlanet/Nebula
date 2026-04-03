using Nebula.Commons.Collections;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class ObjectInitializationExpression
        : Expression
    {
        public ObjectInitializationExpression(SourceCode sourceCode, Token openSquareBracket, TokenSeparatedList<ObjectFieldInitializationExpression> initializators, Token closedSquareBracket)
            : base(sourceCode)
        {
            OpenSquareBracket = openSquareBracket;
            FieldExpressions = initializators;
            ClosedSquareBracket = closedSquareBracket;
        }

        public override NodeType Type => NodeType.ObjectInitializationExpression;

        public Token OpenSquareBracket { get; }
        public TokenSeparatedList<ObjectFieldInitializationExpression> FieldExpressions { get; }
        public Token ClosedSquareBracket { get; }

        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenSquareBracket;

            foreach (var i in FieldExpressions)
            {
                yield return i;
            }

            yield return ClosedSquareBracket;
        }
    }
}
