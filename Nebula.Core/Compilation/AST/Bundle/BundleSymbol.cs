using Nebula.Core.Parsing;
using System.Collections.Immutable;

namespace Nebula.Core.Binding.Symbols
{
    public sealed class BundleSymbol
        : Symbol
    {
        public override SymbolType SymbolType => SymbolType.Bundle;
        public BundleDeclaration Declaration { get; }
        public ImmutableArray<AbstractBundleField> Fields { get; }

        public BundleSymbol(string name, BundleDeclaration declaration, ImmutableArray<AbstractBundleField> fields)
            : base(name)
        {
            Declaration = declaration;
            Fields = fields;
        }
    }
}
