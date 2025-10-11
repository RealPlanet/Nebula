using Nebula.CodeGeneration.Interfaces;
using System.Collections.Generic;

namespace Nebula.CodeGeneration.Definitions
{
    public sealed class NativeMethodDefinition
        : ISupportsComments
    {
        public HashSet<string> LeadingComments { get; } = [];
        public string Name { get; }

        public NativeMethodDefinition(string name)
        {
            Name = name;
        }
    }
}
