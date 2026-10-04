using Nebula.Interop.Enumerators;
using System;

namespace Nebula.CodeGeneration
{
    /// <summary>
    /// Describes the type of a value in the Nebula type system.
    ///
    /// Primitive types (Void, Int, Float, String, Unknown) are shared
    /// singletons with no further shape. Array and Bundle types carry the
    /// extra structural data needed to resolve them recursively: an array's
    /// element type, and a bundle's declared namespace + name.
    /// </summary>
    public sealed class TypeReference : IEquatable<TypeReference>
    {
        public static TypeReference Unknown { get; } = new(TypeIdentifier.Unknown);
        public static TypeReference Void { get; } = new(TypeIdentifier.Void);
        public static TypeReference Bool { get; } = new(TypeIdentifier.Bool);
        public static TypeReference Int { get; } = new(TypeIdentifier.Int32);
        public static TypeReference Float { get; } = new(TypeIdentifier.Float);
        public static TypeReference String { get; } = new(TypeIdentifier.String);

        public TypeIdentifier Identifier { get; }

        /// <summary> Used for array types </summary>
        public TypeReference? ElementType { get; }

        /// <summary> Used for objects </summary>
        public string? SourceNamespace { get; }

        /// <summary> Used for obejcts </summary>
        public string? SourceTypeName { get; }

        internal TypeReference(TypeIdentifier identifier)
            : this(identifier, elementType: null, sourceNamespace: null, sourceTypeName: null)
        {
        }

        private TypeReference(TypeIdentifier identifier, TypeReference? elementType, string? sourceNamespace,
                              string? sourceTypeName)
        {
            Identifier = identifier;
            ElementType = elementType;
            SourceNamespace = sourceNamespace;
            SourceTypeName = sourceTypeName;
        }

        /// <summary>
        /// Builds a fully-resolved array of <paramref name="elementType"/> reference. Nest calls for int[][], int[][][],
        /// etc.
        /// </summary>
        public static TypeReference ArrayOf(TypeReference elementType)
        {
            ArgumentNullException.ThrowIfNull(elementType);
            return new TypeReference(TypeIdentifier.Array, elementType, sourceNamespace: null, sourceTypeName: null);
        }

        /// <summary>
        /// Builds a fully-resolved reference to a object type declared as <paramref name="typeName"/> in
        /// <paramref name="namespace"/>.
        /// </summary>
        public static TypeReference ObjectOf(string @namespace, string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Bundle type name must be provided", nameof(typeName));
            }

            return new TypeReference(TypeIdentifier.Object, elementType: null, @namespace, typeName);
        }

        public bool IsArray => Identifier == TypeIdentifier.Array;
        public bool IsObject => Identifier == TypeIdentifier.Object;
        public bool IsResolved => IsArray ? ElementType is not null : !IsObject || SourceTypeName is not null;

        /// <summary>
        /// The innermost, non-array element type. For int[][][] this returns
        /// Int; for a plain int (or bundle) it returns the type itself.
        /// </summary>
        public TypeReference UnwrapArray()
        {
            TypeReference current = this;
            while (current.IsArray && current.ElementType is not null)
            {
                current = current.ElementType;
            }
            return current;
        }

        /// <summary>
        /// Canonical name: "int", "int[]", "int[][]", "MyNamespace.Object" — used as the interning key in debug
        /// symbol emission.
        /// </summary>
        public string Name
        {
            get
            {
                if (Identifier == TypeIdentifier.Array && ElementType is not null)
                {
                    return $"{ElementType.Name}[]";
                }

                if (Identifier == TypeIdentifier.Object && SourceTypeName is not null)
                {
                    if (string.IsNullOrEmpty(SourceNamespace))
                    {
                        return SourceTypeName;
                    }

                    return $"{SourceNamespace}::{SourceTypeName}";
                }

                return Identifier.ToString().ToLower();
            }
        }

        public string CompiledName
        {
            get
            {
                if(IsArray)
                {
                    return $"array/{Name}";
                }

                if(IsObject)
                {
                    return $"object/{Name}";
                }

                return Name;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(TypeReference? other)
        {
            return other is not null &&
                Identifier == other.Identifier &&
                Equals(ElementType, other.ElementType) &&
                SourceNamespace == other.SourceNamespace &&
                SourceTypeName == other.SourceTypeName;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TypeReference);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier, ElementType, SourceNamespace, SourceTypeName);
        }

        public static bool operator ==(TypeReference? left, TypeReference? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(TypeReference? left, TypeReference? right)
        {
            return !(left == right);
        }
    }
}
