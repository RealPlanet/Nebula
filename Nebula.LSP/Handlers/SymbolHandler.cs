using MediatR;
using Nebula.Core.Compilation;
using Nebula.Core.Compilation.CST.Tree.Declaration;
using Nebula.LSP.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.General;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.LSP.Handlers
{
    internal sealed class SymbolHandler
        : DocumentSymbolHandlerBase
    {
        private readonly CompilationCache _compilationCache;
        private static readonly TextDocumentSelector _documentSelector = new(new TextDocumentFilter
        {
            Pattern = "**/*.nebula",
        });

        public SymbolHandler(CompilationCache compilationCache)
        {
            _compilationCache = compilationCache;
        }

        public override Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
        {
            _compilationCache.CheckCompile();

            var uri = request.TextDocument.Uri.ToUri();
            if (!_compilationCache.TryGetTree(uri, out CompilationUnit? tree))
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer());
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }

            DocumentSymbol rootSymbol = tree.ToSymbol();
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return Task.FromResult(new SymbolInformationOrDocumentSymbolContainer(rootSymbol));
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
        {
            return new DocumentSymbolRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                WorkDoneProgress = false,
            };
        }
    }
}
