using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public class NameExpression
        : Expression
    {
        public override NodeType Type => NodeType.NameExpression;
        public Token Identifier { get; }

        internal NameExpression(SourceCode sourceCode, Token identifier)
            : base(sourceCode)
        {
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
        }
    }
}
