using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration.Bundle;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.AST.Bundle
{
    public sealed class BundleSymbol
        : TypeSymbol
    {
        public override SymbolType SymbolType => SymbolType.Object;
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
