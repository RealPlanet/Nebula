using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;

namespace Nebula.LSP.CompletionAnalyzers
{
    public interface ICompletionProvider
    {
        bool IsApplicable(NebulaCompletionContext context);
        IEnumerable<CompletionItem> Provide(NebulaCompletionContext context);
    }
}
