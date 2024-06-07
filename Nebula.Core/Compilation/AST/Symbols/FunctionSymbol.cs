using Nebula.Core.Parsing;
using System.Collections.Immutable;

namespace Nebula.Core.Binding.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public override SymbolType SymbolType => SymbolType.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public ImmutableArray<AttributeSymbol> Attributes { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionDeclaration? Declaration { get; }
        public NativeFunctionDeclaration? NativeDeclaration { get; }

        internal FunctionSymbol(string name,
                                ImmutableArray<ParameterSymbol> parameters,
                                ImmutableArray<AttributeSymbol> attributes,
                                TypeSymbol returnType,
                                BaseFunctionDeclaration? declaration = null)
            : base(name)
        {
            Parameters = parameters;
            Attributes = attributes;
            ReturnType = returnType;

            if (declaration is NativeFunctionDeclaration nfd)
                NativeDeclaration = nfd;

            if (declaration is FunctionDeclaration fd)
                Declaration = fd;
        }
    }
}
