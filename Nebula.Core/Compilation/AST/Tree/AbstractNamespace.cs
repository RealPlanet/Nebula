using Nebula.Core.Parsing;

namespace Nebula.Core.Binding
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
