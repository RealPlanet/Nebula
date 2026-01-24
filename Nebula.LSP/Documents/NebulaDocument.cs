using Nebula.Commons.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace Nebula.LSP.Documents
{
    public class NebulaDocument
    {
        public SourceCode Text { get; internal set; }
        public string LanguageId { get; }
        public int? Version { get; }
        internal Uri Uri { get; }

        public static NebulaDocument CreateFrom(TextDocumentItem textDocument)
        {
            return new NebulaDocument(textDocument.LanguageId, textDocument.Uri.ToUri(), textDocument.Version, textDocument.Text);
        }

        public NebulaDocument(string languageId, Uri uri, int? version, string text)
        {
            LanguageId = languageId;
            Uri = uri;
            Version = version;
            Text = SourceCode.From(text, uri.ToString());
        }

        public void ApplyChange(DocumentPosition start, DocumentPosition end, string text)
        {
            Text.Replace(start, end, text);
        }

        public void ApplyChange(string text)
        {
            Text.Replace(text);
        }
    }
}
