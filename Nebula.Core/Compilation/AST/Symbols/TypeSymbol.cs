using Nebula.CodeEmitter.Types;

namespace Nebula.Core.Binding.Symbols
{
    public sealed class TypeSymbol
        : Symbol
    {
        #region Static
        public static readonly TypeSymbol Error = new("?");
        //public static readonly TypeSymbol Any = new("any");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");
        public static readonly TypeSymbol Bundle = new("bundle");

        public string? Alias { get; private set; }
        public string? Namespace { get; private set; }
        public bool IsNamedBundle => IsBundle && !string.IsNullOrEmpty(Alias);

        public bool IsError => this == Error;
        public bool IsBool => this == Bool;
        public bool IsInt => this == Int;
        public bool IsString => this == String;
        public bool IsVoid => this == Void;
        public bool IsBundle => this == Bundle;

        public static TypeSymbol TypeFromEnum(TypeIdentifier identifier)
        {
            switch (identifier)
            {
                case TypeIdentifier.Void:
                    return TypeSymbol.Void;
                case TypeIdentifier.Bool:
                    return TypeSymbol.Bool;
                case TypeIdentifier.Int32:
                    return TypeSymbol.Int;
                case TypeIdentifier.String:
                    return TypeSymbol.String;
                case TypeIdentifier.Bundle:
                    return TypeSymbol.Bundle;
                default:
                    throw new System.Exception($"Unknown type: {identifier}");
            }
        }

        public static TypeSymbol GetNamedBundleType(string @namespace, string bundleType)
        {
            return new TypeSymbol(Bundle.Name)
            {
                Namespace = @namespace,
                Alias = bundleType
            };
        }
        #endregion

        public override SymbolType SymbolType => SymbolType.Type;
        private TypeSymbol(string name)
            : base(name)
        {
        }

        public override string ToString() => Name;

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is TypeSymbol s)
            {
                return s.Name == Name;
            }

            return false;
        }

        public static bool operator ==(TypeSymbol a, TypeSymbol b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(TypeSymbol a, TypeSymbol b)
        {
            return !(a == b);
        }
    }
}
