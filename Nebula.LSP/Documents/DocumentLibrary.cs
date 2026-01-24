using Nebula.Commons.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

namespace Nebula.LSP.Documents
{
    public class DocumentLibrary
        : IEnumerable<NebulaDocument>
    {
        public delegate void DocumentChangedEventHandler(NebulaDocument document);
        public event DocumentChangedEventHandler? DocumentChanged;

        private readonly ConcurrentDictionary<Uri, NebulaDocument> _documents = [];

        public void AddDocument(NebulaDocument textDocument)
        {
            _documents.AddOrUpdate(textDocument.Uri, textDocument, (k, v) => textDocument);
            DocumentChanged?.Invoke(textDocument);
        }

        public void UpdateDocument(Uri uri, string text)
        {
            if (!TryGetDocument(uri, out NebulaDocument? document))
            {
                // TODO Log
                return;
            }

            document.ApplyChange(text);
            DocumentChanged?.Invoke(document);
        }

        internal void UpdateDocument(Uri uri, Container<TextDocumentContentChangeEvent> contentChanges)
        {
            if (!TryGetDocument(uri, out NebulaDocument? document))
            {
                // TODO Log
                return;
            }

            foreach (var change in contentChanges)
            {
                Debug.Assert(change.Range != null);
                DocumentPosition start = new(change.Range.Start.Line, change.Range.Start.Character);
                DocumentPosition end = new(change.Range.End.Line, change.Range.End.Character);

                document.ApplyChange(start, end, change.Text);
            }

            DocumentChanged?.Invoke(document);
        }

        public void RemoveDocument(Uri uri)
        {
            _documents.TryRemove(uri, out _);
        }

        public bool TryGetDocument(Uri uri, [NotNullWhen(true)] out NebulaDocument? document)
        {
            return _documents.TryGetValue(uri, out document);
        }

        public IEnumerator<NebulaDocument> GetEnumerator()
        {
            return _documents.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
