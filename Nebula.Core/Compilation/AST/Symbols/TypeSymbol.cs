using Nebula.CodeEmitter.Types;

namespace Nebula.Core.Binding.Symbols
{
    public class TypeSymbol
        : Symbol
    {
        #region Static
        public static readonly TypeSymbol Error = new("?");
        //public static readonly TypeSymbol Any = new("any");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol Float = new("float");
        public static readonly TypeSymbol String = new("string");
        public static readonly TypeSymbol Void = new("void");
        public static readonly TypeSymbol BaseObject = new("object");
        public static readonly TypeSymbol BaseArray = new("array");

        public bool IsError => this == Error;
        public bool IsBool => this == Bool;
        public bool IsInt => this == Int;
        public bool IsString => this == String;
        public bool IsVoid => this == Void;
        public bool IsArray => this is ArrayTypeSymbol;

        public bool IsObject => this is ObjectTypeSymbol;

        public static TypeSymbol TypeFromEnum(TypeIdentifier identifier)
        {
            switch (identifier)
            {
                case TypeIdentifier.Void:
                    return TypeSymbol.Void;
                case TypeIdentifier.Int32:
                    return TypeSymbol.Int;
                case TypeIdentifier.String:
                    return TypeSymbol.String;
                default:
                    throw new System.Exception($"Unknown type: {identifier}");
            }
        }

        #endregion

        public override SymbolType SymbolType => SymbolType.Type;
        protected TypeSymbol(string name)
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
