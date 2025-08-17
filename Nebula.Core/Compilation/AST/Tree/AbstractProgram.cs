using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Binding.Referencing;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Statements;
using System.Collections.Generic;


namespace Nebula.Core.Compilation.AST.Tree
{
    public sealed class AbstractProgram
    {
        public SourceCode SourceCode { get; }
        public AbstractNamespace Namespace { get; private set; }
        public Dictionary<FunctionSymbol, AbstractBlockStatement> Functions { get; } = new();

        /// <summary> Native functions don't have bodies as they're bound at runtime </summary>
        public HashSet<FunctionSymbol> NativeFunctions { get; } = new();

        public Dictionary<string, BundleSymbol> Bundles { get; } = new();

        public AbstractProgramReferences References { get; }

        public AbstractProgram(SourceCode sourceCode, AbstractNamespace ns)
        {
            SourceCode = sourceCode;
            Namespace = ns;
            References = new(this);
        }
    }
}
