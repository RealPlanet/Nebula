using Nebula.Core.Binding.Symbols;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class AbstractBundleDefinition
    {
        public string Namespace { get; }
        public string Name { get; }

        public Dictionary<string, TypeSymbol> Fields { get; } = new();

        public AbstractBundleDefinition(string @namespace, string name)
        {
            Namespace = @namespace;
            Name = name;
        }
    }
}
