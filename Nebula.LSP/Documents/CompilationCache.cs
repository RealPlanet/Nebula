using Nebula.Commons.Reporting;
using Nebula.Commons.Text;
using Nebula.Core.Compilation;
using Nebula.Core.Compilation.AST.Binding;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.CST.Parsing;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nebula.LSP.Documents
{
    internal class CompilationCache
    {
        public IReadOnlyList<AbstractProgram> Programs => _programs.Values.ToList();
        public IReadOnlyList<CompilationUnit> ProgramTrees => _programTrees.Values.ToList();

        private readonly ConcurrentDictionary<Uri, AbstractProgram> _programs = new();
        private readonly ConcurrentDictionary<Uri, CompilationUnit> _programTrees = new();

        private readonly DocumentLibrary _documentLibrary;
        private readonly Serilog.ILogger _logger;
        private bool _requiresCompilation = false;
        private readonly Compiler.Options _compileOptions = new()
        {
            EmitProgram = false,
            OutputFolder = string.Empty,
            OutputToSourceLocation = true,
            ReadableBytecode = true,
        };

        public CompilationCache(DocumentLibrary documentLibrary)
        {
            _logger = Log.Logger;
            _documentLibrary = documentLibrary;
            _documentLibrary.DocumentChanged += d => _requiresCompilation = true;
        }

        public bool CheckCompile()
        {
            if (!_requiresCompilation)
            {
                return true;
            }
            _requiresCompilation = false;
            _compileOptions.Sources.Clear();
            foreach (NebulaDocument doc in _documentLibrary)
            {
                _compileOptions.Sources.Add(doc.Text);
            }

            List<CompilationUnit> compileUnits = [];
            foreach (SourceCode source in _compileOptions.Sources)
            {
                CompilationUnit tree = Parser.Parse(source, out Report _);
                compileUnits.Add(tree);
                AddTree(new(source.FileName), tree);
            }

            ICollection<AbstractProgram> programs = Binder.Bind(compileUnits, [], out Report? bindingReport);
            foreach (AbstractProgram script in programs)
            {
                Uri uri = new(script.SourceCode.FileName);
                AddProgram(uri, script);
            }

            return true;
        }

        public bool TryGetProgram(Uri uri, [NotNullWhen(true)] out AbstractProgram? value)
        {
            return _programs.TryGetValue(uri, out value);
        }

        public bool TryGetTree(Uri uri, [NotNullWhen(true)] out CompilationUnit? value)
        {
            return _programTrees.TryGetValue(uri, out value);
        }

        public void AddTree(Uri uri, CompilationUnit unit)
        {
            _programTrees[uri] = unit;
        }

        public void AddProgram(Uri uri, AbstractProgram script)
        {
            _programs[uri] = script;
        }

        public void Clear()
        {
            _programs.Clear();
            _programTrees.Clear();
        }
    }
}
