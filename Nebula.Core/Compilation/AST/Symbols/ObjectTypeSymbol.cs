namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class ObjectTypeSymbol
        : TypeSymbol
    {
        public ObjectTypeSymbol(string @namespace, string name)
            : base(name)
        {
            Namespace = @namespace;
        }

        public string Namespace { get; }
    }
}
