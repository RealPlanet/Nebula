using System;

namespace Nebula.Core.Compilation.AST.Binding.Bundles
{
    public struct BundleIdentifier : IEquatable<BundleIdentifier>
    {
        public string Namespace { get; set; }
        public string Name { get; set; }

        public override readonly bool Equals(object? obj) => obj is BundleIdentifier identifier && Equals(identifier);
        public readonly bool Equals(BundleIdentifier other) => Namespace == other.Namespace && Name == other.Name;
        public override readonly int GetHashCode() => HashCode.Combine(Namespace, Name);

        public static bool operator ==(BundleIdentifier left, BundleIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BundleIdentifier left, BundleIdentifier right)
        {
            return !(left == right);
        }
    }
}
