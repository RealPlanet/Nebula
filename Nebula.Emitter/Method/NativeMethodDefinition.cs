using System.Collections.Generic;

namespace Nebula.CodeEmitter
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
