using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Interop.Structures;
using Nebula.LSP.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        public CompletionHandler(CompilationCache cache, DocumentLibrary documentLibrary)
        {
            _compilationCache = cache;
            _documentLibrary = documentLibrary;
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

            bool isOutSideAnyFunction = IsOutsideFunction(request, cursorPositionSpan, tree);
            if (isOutSideAnyFunction)
            {
                return await GenerateGlobalCompletionItemsAsync(request, cursorPositionSpan, tree, cancellationToken);
            }

            return await GenerateFunctionScopeCompletionItemsAsync(request, cursorPositionSpan, tree, cancellationToken);
        }

        private bool IsOutsideFunction(CompletionParams request, TextSpan cursorPositionSpan, AbstractProgram tree)
        {
            foreach (var function in tree.Functions.Values)
            {
                if (cursorPositionSpan.OverlapsWith(function.OriginalNode.Span))
                    return false;
            }

            return true;
        }

        private static async Task<CompletionList> GenerateFunctionScopeCompletionItemsAsync(CompletionParams request,
                                                                                            TextSpan cursorPositionSpan,
                                                                                            AbstractProgram tree,
                                                                                            CancellationToken cancellationToken)
        {
            List<CompletionItem> items = new();

            foreach (var kvp in tree.References.AllPrograms)
            {
                var import = kvp.Value;
                var ns = kvp.Key;

                if (ns == tree.Namespace.Text)
                    continue;

                foreach (var bundle in import.Bundles.Values)
                {
                    var name = $"{import.Namespace.Text}::{bundle.Name}";
                    items.Add(new CompletionItem
                    {
                        Kind = CompletionItemKind.Class,
                        InsertText = name,
                        InsertTextFormat = InsertTextFormat.PlainText,
                        InsertTextMode = InsertTextMode.AsIs,
                        Label = name,
                    });
                }

                foreach (var func in import.Functions)
                {
                    var name = $"{import.Namespace.Text}::{func.Key.Name}";
                    items.Add(new CompletionItem
                    {
                        Kind = CompletionItemKind.Function,
                        InsertText = name,
                        InsertTextFormat = InsertTextFormat.PlainText,
                        InsertTextMode = InsertTextMode.AsIs,
                        Label = name,
                    });
                }
            }

            foreach (KeyValuePair<FunctionSymbol, AbstractBlockStatement> function in tree.Functions)
            {
                FunctionSymbol symbol = function.Key;
                AbstractBlockStatement body = function.Value;

                items.Add(new CompletionItem
                {
                    Kind = CompletionItemKind.Function,
                    InsertText = function.Key.Name,
                    InsertTextFormat = InsertTextFormat.PlainText,
                    InsertTextMode = InsertTextMode.AsIs,
                    Label = function.Key.Name,
                });

                if (!body.OriginalNode.Span.OverlapsWith(cursorPositionSpan))
                {
                    continue;
                }

                foreach (VariableSymbol variables in symbol.FunctionScope.GetDeclaredVariablesWithParent())
                {
                    items.Add(new CompletionItem
                    {
                        Kind = CompletionItemKind.Variable,
                        InsertText = variables.Name,
                        InsertTextFormat = InsertTextFormat.PlainText,
                        InsertTextMode = InsertTextMode.AsIs,
                        Label = variables.Name,
                    });
                }
            }

            var result = new CompletionList(items, false);
            return result;
        }

        private async Task<CompletionList> GenerateGlobalCompletionItemsAsync(CompletionParams request, TextSpan cursorPositionSpan, AbstractProgram tree, CancellationToken cancellationToken)
        {
            List<CompletionItem> result = [];
            bool generateImportStatements = true;
            if (request.Context != null)
            {
                if (request.Context.TriggerCharacter != null)
                {
                    generateImportStatements = "import".Contains(request.Context.TriggerCharacter);
                }
            }

            var sourceUri = new Uri(tree.SourceCode.FileName);

            if (generateImportStatements)
            {
                foreach (var document in _documentLibrary)
                {
                    if (document.Uri == sourceUri)
                        continue;

                    if (!_compilationCache.TryGetTree(document.Uri, out var otherTree))
                        continue;

                    result.Add(new CompletionItem
                    {
                        Kind = CompletionItemKind.Module,
                        InsertText = $"import {otherTree.NamespaceStatement.Namespace.Text};\n",
                        InsertTextMode = InsertTextMode.AsIs,
                        InsertTextFormat = InsertTextFormat.PlainText,
                        Label = $"import {otherTree.NamespaceStatement.Namespace.Text}",
                    });
                }
            }

            return new(result);
        }

        protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false,
            };
        }
    }
}
