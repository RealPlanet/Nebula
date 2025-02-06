namespace Nebula.CodeEmitter.Types
{
    public class TypeReference
    {
        public static TypeReference Unknown { get; } = new(TypeIdentifier.Unknown);
        public static TypeReference Void { get; } = new(TypeIdentifier.Void);
        //public static TypeReference Char { get; } = new(TypeIdentifier.Char);
        public static TypeReference Bool { get; } = new(TypeIdentifier.Int32);
        public static TypeReference Int { get; } = new(TypeIdentifier.Int32);
        public static TypeReference Float { get; } = new(TypeIdentifier.Float);
        public static TypeReference String { get; } = new(TypeIdentifier.String);
        public static TypeReference Bundle { get; } = new(TypeIdentifier.Bundle);

        public TypeIdentifier Identifier { get; }

        public string Name => Identifier.ToString().ToLower();

        internal TypeReference(TypeIdentifier identifier)
        {
            Identifier = identifier;
        }

        public override string ToString() => Name;
    }
}
