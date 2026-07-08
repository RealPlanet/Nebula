using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Interop.Enumerators;
using Nebula.LSP.CompletionAnalyzers;
using Nebula.LSP.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.LSP.Handlers
{
    internal class CompletionHandler
        : CompletionHandlerBase
    {
        private readonly CompilationCache _compilationCache;
        private readonly DocumentLibrary _documentLibrary;
        private readonly TextDocumentSelector _documentSelector = new(new TextDocumentFilter
        {
            Pattern = "**/*.nebula",
        });

        private readonly List<ICompletionProvider> _completionProviders = [];


        public CompletionHandler(CompilationCache cache, DocumentLibrary documentLibrary)
        {
            _compilationCache = cache;
            _documentLibrary = documentLibrary;

            _completionProviders.Add(new ImportStatementProvider());
            _completionProviders.Add(new AvailableLocalFunctionsProvider());
            _completionProviders.Add(new LocalVariablesProvider());
            _completionProviders.Add(new ObjectMemberFieldsProvider());
        }

        public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
        {
            // Not sure about this one
            return Task.FromResult(request);
        }

        public override async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            _compilationCache.CheckCompile();
            var uri = request.TextDocument.Uri.ToUri();

            if (!_compilationCache.TryGetProgram(uri, out AbstractProgram? tree))
            {
                return [];
            }

            Position reqPos = request.Position;
            TextLine startLine = tree.SourceCode.Lines[reqPos.Line];
            var startChar = startLine.Start + reqPos.Character;
            var cursorPositionSpan = new TextSpan(startChar, 1);

            List<CompletionItem> resultItems = [];
            var context = BuildContext(uri, tree, cursorPositionSpan, request.Context?.TriggerCharacter);
            foreach(var processor in _completionProviders)
            {
                if(processor.IsApplicable(context))
                {
                    var items = processor.Provide(context);
                    resultItems.AddRange(items);
                }
            }

            return new(resultItems);
        }

        private NebulaCompletionContext BuildContext(Uri uri, AbstractProgram tree, TextSpan cursorPositionSpan, string? triggerCharacter)
        {
            return new NebulaCompletionContext(_documentLibrary, _compilationCache, uri, tree, cursorPositionSpan, triggerCharacter);
        }

        //    if (currentFunction.Value)
        //    {
        //        var deepestNode = nodePath.Last();

        //        switch (deepestNode.Type)
        //        {
        //            case AbstractNodeType.ObjectFieldInitializationExpression:
        //                {
        //                    var parentNode = nodePath[nodePath.Count - 2];
        //                    if (parentNode.Type == AbstractNodeType.ObjectInitializationExpression)
        //                    {
        //                        //AnalyzeObjectInitialization(items, tree, (AbstractObjectInitializationExpression)parentNode);
        //                    }

        //                    canInsertFunctionHere = false;
        //                    break;
        //                }

        //            case AbstractNodeType.ObjectInitializationExpression:
        //                {
        //                    AnalyzeObjectInitialization(items, tree, (AbstractObjectInitializationExpression)deepestNode);
        //                    canInsertFunctionHere = false;
        //                    break;
        //                }
        //        }
        //    }

        protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false,
                TriggerCharacters = new Container<string>(["."])
            };
        }
    }
}
