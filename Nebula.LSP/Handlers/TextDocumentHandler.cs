

using MediatR;
using Nebula.Commons.Text;
using Nebula.LSP.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.LSP.Handlers
{
    internal class TextDocumentHandler
        : TextDocumentSyncHandlerBase
    {
        private readonly DocumentLibrary _documentLibrary;

        private TextDocumentSyncKind DocumentSyncType { get; } = TextDocumentSyncKind.Full;
        private readonly TextDocumentSelector _documentSelector = new(new TextDocumentFilter
        {
            Pattern = "**/*.nebula",
        });

        public TextDocumentHandler(DocumentLibrary documentLibrary)
        {
            _documentLibrary = documentLibrary;
        }

        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentSyncRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                Change = DocumentSyncType,
            };
        }

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        {
            return new TextDocumentAttributes(uri, "nebula");
        }

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            _documentLibrary.AddDocument(NebulaDocument.CreateFrom(request.TextDocument));
            return Unit.Task;
        }

        public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {


            if (DocumentSyncType == TextDocumentSyncKind.Full)
            {
                Debug.Assert(request.ContentChanges.Count() == 1);
                TextDocumentContentChangeEvent change = request.ContentChanges.First();
                _documentLibrary.UpdateDocument(request.TextDocument.Uri.ToUri(), change.Text);
                return Unit.Task;
            }

            _documentLibrary.UpdateDocument(request.TextDocument.Uri.ToUri(), request.ContentChanges);
            return Unit.Task;
        }
        
        public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            var documentUri = request.TextDocument.Uri.ToUri();
            _documentLibrary.RemoveDocument(documentUri);
            return Unit.Task;
        }
    }
}
