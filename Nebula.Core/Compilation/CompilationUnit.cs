using Nebula.Commons.Text;
using Nebula.Core.Parsing;
using Nebula.Core.Parsing.Statements;
using System.Collections.Generic;

namespace Nebula.Core.Compilation
{
    /// <summary>Root of the concrete syntax tree</summary>
    public sealed class CompilationUnit
    {
        /// <summary>Namespace of this compilation unit</summary>
        public NamespaceStatement NamespaceStatement { get; set; } = NamespaceStatement.Empty;

        /// <summary>All the namespaces this unit wants to reference</summary>
        public IList<ImportStatement> Imports { get; } = new List<ImportStatement>();

        /// <summary>Functions defined in this compilation unit</summary>
        public IList<FunctionDeclaration> Functions { get; } = new List<FunctionDeclaration>();

        /// <summary>Native function declaration which will be used by the binder to resolve native function calls</summary>
        public IList<NativeFunctionDeclaration> NativeFunction { get; } = new List<NativeFunctionDeclaration>();

        /// <summary>Bundles defined in this compilation unit</summary>
        public IList<BundleDeclaration> Bundles { get; } = new List<BundleDeclaration>();

        /// <summary> The source code for this particular compilation unit</summary>
        public SourceCode Source { get; }

        public CompilationUnit(SourceCode source)
        {
            Source = source;
        }

        public void Clear()
        {
            NamespaceStatement = NamespaceStatement.Empty;
            Imports.Clear();
            Functions.Clear();
            NativeFunction.Clear();
            Bundles.Clear();
        }
    }
}
