using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    public sealed class BundleDefinition
        : ISupportsComments
    {
        public HashSet<string> LeadingComments { get; } = [];

        public string Name { get; }
        public IList<ParameterDefinition> Fields { get; } = [];

        public BundleDefinition(string name)
        {
            Name = name;
        }
    }
}
