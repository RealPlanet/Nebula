using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Declaration.Bundle;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Statements;
using System.Collections;
using System.Collections.Generic;

namespace Nebula.Core.Compilation
{
    /// <summary>Root of the concrete syntax tree</summary>
    public sealed class CompilationUnit
        : IEnumerable<Node>
    {
        /// <summary>Namespace of this compilation unit</summary>
        public NamespaceStatement NamespaceStatement { get; set; } = NamespaceStatement.Empty;

        /// <summary>All the namespaces this unit wants to reference</summary>
        public IList<ImportStatement> Imports { get; } = new List<ImportStatement>();

        /// <summary>Functions defined in this compilation unit</summary>
        public IList<FunctionDeclaration> Functions { get; } = new List<FunctionDeclaration>();

        /// <summary>Native function declaration which will be used by the binder to resolve native function calls</summary>
        public IList<NativeFunctionDeclaration> NativeFunctions { get; } = new List<NativeFunctionDeclaration>();

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
            NativeFunctions.Clear();
            Bundles.Clear();
        }

        public IEnumerator<Node> GetEnumerator()
        {
            yield return NamespaceStatement;
            foreach (var n in Imports)
                yield return n;

            foreach (var n in Functions)
                yield return n;

            foreach (var n in NativeFunctions)
                yield return n;

            foreach (var n in Bundles)
                yield return n;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
