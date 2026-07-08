using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;

namespace Nebula.LSP.CompletionAnalyzers
{
    public class ImportStatementProvider
        : ICompletionProvider
    {
        public bool IsApplicable(NebulaCompletionContext context)
            => context.IsInFunction == false;

        public IEnumerable<CompletionItem> Provide(NebulaCompletionContext context)
        {
            LPSLogger.Logger.Information("Generating import statements");
            foreach (var program in context.OtherPrograms)
            {
                yield return new CompletionItem
                {
                    Kind = CompletionItemKind.Module,
                    InsertText = $"import {program.Namespace.Text};\n",
                    InsertTextMode = InsertTextMode.AsIs,
                    InsertTextFormat = InsertTextFormat.PlainText,
                    Label = $"import {program.Namespace.Text}",
                };
            }
        }
    }
}