using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Binding;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Interop.SafeHandles;
using Nebula.LSP.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.LSP.CompletionAnalyzers
{
    public sealed class NebulaCompletionContext
    {
        public DocumentLibrary DocumentLibrary { get; }
        public CompilationCache CompilationCache { get; }
        public Uri SourceUri { get; }
        public AbstractProgram Program { get; }
        public Scope? CurrentScope { get; set; }
        public List<AbstractNode> NodePath { get; } = [];
        public TextSpan CurrentPosition { get; set; }
        public FunctionSymbol? CurrentFunctionSymbol { get; set; }
        public AbstractBlockStatement? CurrentFunctionBody { get; set; }

        public IEnumerable<AbstractProgram> OtherPrograms
        {
            get
            {
                foreach (var document in DocumentLibrary)
                {
                    if (document.Uri == SourceUri)
                    {
                        continue;
                    }

                    if (!CompilationCache.TryGetProgram(document.Uri, out var program))
                    {
                        continue;
                    }

                    yield return program;
                }
            }
        }

        public NebulaCompletionContext(DocumentLibrary documentLibrary,
                                       CompilationCache compilationCache,
                                       Uri sourceUri,
                                       AbstractProgram program,
                                       TextSpan currentPosition,
                                       string? triggerCharacter)
        {
            DocumentLibrary = documentLibrary;
            CompilationCache = compilationCache;
            SourceUri = sourceUri;
            Program = program;
            CurrentPosition = currentPosition;
            TriggerCharacter = triggerCharacter;

            var currentFunction
                = program.Functions.FirstOrDefault(f => f.Value.OriginalNode.FullSpan.OverlapsWith(CurrentPosition));

            CurrentFunctionSymbol = currentFunction.Key;
            CurrentFunctionBody = currentFunction.Value;

            if (CurrentFunctionBody != null)
            {
                CurrentFunctionBody.FindNodePathToCursor(CurrentPosition, NodePath);
                CurrentScope = CurrentFunctionSymbol.FunctionScope;
                IsInFunction = true;
            }
            else
            {
                IsInFunction = false;
                CurrentScope = null;
            }
        }

        public string? TriggerCharacter { get; private set; }
        public bool IsInFunction { get; private set; }

        private bool? _isObjectAccess;
        public bool IsObjectAccess
        {
            get
            {
                if (_isObjectAccess is null)
                {
                    _isObjectAccess = false;
                }

                return (bool)_isObjectAccess;
            }
        }
    }
}
