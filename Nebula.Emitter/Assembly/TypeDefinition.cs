using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    /// <summary>
    /// Type definition of a compiled program
    /// </summary>
    public sealed class TypeDefinition
    {
        public ICollection<BundleDefinition> Bundles { get; } = [];
        public ICollection<MethodDefinition> Methods { get; } = [];
    }
}
