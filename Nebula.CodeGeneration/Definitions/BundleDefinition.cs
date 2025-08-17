using Nebula.CodeGeneration.Interfaces;
using System.Collections.Generic;

namespace Nebula.CodeGeneration.Definitions
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
