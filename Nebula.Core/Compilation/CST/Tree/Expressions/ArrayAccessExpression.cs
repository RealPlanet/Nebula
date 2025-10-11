using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class ArrayAccessExpression
        : NameExpression
    {
        public override NodeType Type => NodeType.ArrayAccessExpression;
        public Token OpenSquare { get; }
        public Expression AccessExpression { get; }
        public Token CloseSquare { get; }

        public ArrayAccessExpression(SourceCode sourceCode, Token identifier, Token openSquare, Expression accessExpression, Token closeSquare)
            : base(sourceCode, identifier)
        {
            OpenSquare = openSquare;
            AccessExpression = accessExpression;
            CloseSquare = closeSquare;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return OpenSquare;
            yield return AccessExpression;
            yield return CloseSquare;
        }
    }
}
