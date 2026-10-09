using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class IndexExpression
        : Expression
    {
        public override NodeType Type => NodeType.IndexExpression;
        public Expression Target { get; }
        public Token OpenSquare { get; }
        public Expression Index { get; }
        public Token CloseSquare { get; }

        public IndexExpression(SourceCode sourceCode,
                               Expression target,
                               Token openSquare,
                               Expression index,
                               Token closeSquare)
            : base(sourceCode)
        {
            Target = target;
            OpenSquare = openSquare;
            Index = index;
            CloseSquare = closeSquare;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (var c in Target.GetChildren())
                yield return c;
            yield return OpenSquare;
            foreach (var c in Index.GetChildren())
                yield return c;
            yield return CloseSquare;
        }
    }
}
