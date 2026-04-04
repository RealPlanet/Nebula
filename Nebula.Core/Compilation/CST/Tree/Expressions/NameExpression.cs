using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public class NameExpression
        : Expression
    {
        public override NodeType Type => NodeType.NameExpression;

        public Token? Namespace { get; }
        public Token? Separator { get; }
        public Token Identifier { get; }

        public NameExpression(SourceCode sourceCode, Token identifier)
            : this(sourceCode, null, null, identifier)
        {
        }

        public NameExpression(SourceCode sourceCode, Token? @namespace, Token? separator, Token identifier)
            : base(sourceCode)
        {
            Namespace = @namespace;
            Separator = separator;
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            if (Namespace != null)
                yield return Namespace;
            if (Separator != null)
                yield return Separator;
            yield return Identifier;
        }
    }
}
