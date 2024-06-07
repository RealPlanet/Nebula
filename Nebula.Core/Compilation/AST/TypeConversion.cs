using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Compilation.AST
{
    public sealed class TypeConversion
    {
        public static readonly TypeConversion None = new(exists: false, isIdentity: false, isImplicit: false);
        public static readonly TypeConversion Identity = new(exists: true, isIdentity: true, isImplicit: true);
        public static readonly TypeConversion Implict = new(exists: true, isIdentity: false, isImplicit: true);
        public static readonly TypeConversion Explicit = new(exists: true, isIdentity: false, isImplicit: false);
        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;
        private TypeConversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public static TypeConversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Identity;

            //if (from != TypeSymbol.Void && to == TypeSymbol.Any)
            //    return Implict;

            //if (from == TypeSymbol.Any && to != TypeSymbol.Void)
            //    return Explicit;

            // Default initializer, lookup name because bundles create a typesymbol on the stop with an alias
            if (from == TypeSymbol.Int && to.Name == TypeSymbol.Bundle.Name)
                return Implict;

            if (from == TypeSymbol.Int || from == TypeSymbol.Bool)
            {
                if (to == TypeSymbol.String)
                    return Explicit;
            }

            if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Int || to == TypeSymbol.Bool)
                    return Explicit;
            }

            return None;
        }
    }

}
