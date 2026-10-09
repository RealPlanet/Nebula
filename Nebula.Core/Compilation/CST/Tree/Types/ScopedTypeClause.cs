using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Types
{
    public class ScopedTypeClause
        : BaseTypeClause
    {
        public override NodeType Type => NodeType.ObjectTypeClause;

        public Token Namespace { get; }
        public Token DoubleColon { get; }
        public Token ClassName { get; }

        public ScopedTypeClause(SourceCode sourceCode, Token @namespace, Token doubleColon, Token classtName)
            : base(sourceCode)
        {
            Namespace = @namespace;
            DoubleColon = doubleColon;
            ClassName = classtName;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Namespace;
            yield return DoubleColon;
            yield return ClassName;
        }
    }
}
