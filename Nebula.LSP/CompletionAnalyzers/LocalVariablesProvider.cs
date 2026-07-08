using Nebula.Core.Compilation.AST.Symbols;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;

namespace Nebula.LSP.CompletionAnalyzers
{
    public class LocalVariablesProvider
        : ICompletionProvider
    {
        public bool IsApplicable(NebulaCompletionContext context)
        {
            return context.IsInFunction;
        }

        public IEnumerable<CompletionItem> Provide(NebulaCompletionContext context)
        {
            var funcSymbol = context.CurrentFunctionSymbol;
            if (funcSymbol is null)
            {
                LPSLogger.Logger.Error($"Function symbol is null but context is marked as inside a function!?");
                throw new Exception("Function symbol is null but context is marked as inside a function!?");
            }

            foreach (VariableSymbol variables in funcSymbol.FunctionScope.GetDeclaredVariables())
            {
                yield return new CompletionItem
                {
                    Kind = CompletionItemKind.Variable,
                    InsertText = variables.Name,
                    InsertTextFormat = InsertTextFormat.PlainText,
                    InsertTextMode = InsertTextMode.AsIs,
                    Label = variables.Name,
                };
            }
        }
    }
}