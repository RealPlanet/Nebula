using Nebula.CodeGeneration.Definitions;
using System.Collections.Generic;

namespace Nebula.CodeGeneration
{
    /// <summary>
    /// Type definition of a compiled program
    /// </summary>
    public sealed class TypeDefinition
    {
        public ICollection<BundleDefinition> Bundles { get; } = [];
        public ICollection<MethodDefinition> Methods { get; } = [];
        public ICollection<NativeMethodDefinition> NativeMethods { get; } = [];
    }
}
