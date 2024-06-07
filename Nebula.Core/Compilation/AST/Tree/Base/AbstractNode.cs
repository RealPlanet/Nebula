using Nebula.Commons.Syntax;

namespace Nebula.Core.Binding
{
    public abstract class AbstractNode
    {
        /// <summary>
        /// The type of node rapresented by an enum value
        /// </summary>
        public abstract AbstractNodeType Type { get; }
        public Node OriginalNode { get; }

        protected AbstractNode(Node node)
        {
            OriginalNode = node;
        }
    }
}
