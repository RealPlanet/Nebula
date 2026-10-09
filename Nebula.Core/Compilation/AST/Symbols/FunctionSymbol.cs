using Nebula.Core.Compilation.AST.Binding;
using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public override SymbolType SymbolType => SymbolType.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public ImmutableArray<AttributeSymbol> Attributes { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionDeclaration? Declaration { get; }
        public NativeFunctionDeclaration? NativeDeclaration { get; }

        public Scope FunctionScope { get; }

        internal FunctionSymbol(string name,
                                ImmutableArray<ParameterSymbol> parameters,
                                ImmutableArray<AttributeSymbol> attributes,
                                TypeSymbol returnType,
                                Scope scope,
                                BaseFunctionDeclaration? declaration = null)
            : base(name)
        {
            FunctionScope = scope;
            Parameters = parameters;
            Attributes = attributes;
            ReturnType = returnType;

            if (declaration is NativeFunctionDeclaration nfd)
            {
                NativeDeclaration = nfd;
            }

            if (declaration is FunctionDeclaration fd)
            {
                Declaration = fd;
            }
        }
    }
}
