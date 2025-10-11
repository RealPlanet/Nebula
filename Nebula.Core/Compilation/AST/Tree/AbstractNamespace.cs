using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Statements;

namespace Nebula.Core.Compilation.AST.Tree
{
    public sealed class AbstractNamespace
        : AbstractNode
    {
        public override AbstractNodeType Type => AbstractNodeType.NamespaceDeclaration;

        public string Text { get; }

        public AbstractNamespace(NamespaceStatement namespaceStatement)
             : base(namespaceStatement)
        {
            Text = namespaceStatement.Namespace.Text.Trim('"');
        }
    }
}
