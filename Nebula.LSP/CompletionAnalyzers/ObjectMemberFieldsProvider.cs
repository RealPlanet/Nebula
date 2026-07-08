using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.LSP.CompletionAnalyzers
{
    public class ObjectMemberFieldsProvider
        : ICompletionProvider
    {
        public bool IsApplicable(NebulaCompletionContext context)
        {
            return context.IsInFunction &&
                context.IsObjectAccess;
        }

        public IEnumerable<CompletionItem> Provide(NebulaCompletionContext context)
        {
            var nodeLastNode = context.NodePath.Last();
            return [];
        }

        private IEnumerable<CompletionItem> GenerateObjectCompletionItems(BundleSymbol bundle, IEnumerable<AbstractBundleField> fieldsToIgnore)
        {
            foreach (var missingField in bundle.Fields.Where(f => !fieldsToIgnore.Contains(f)))
            {
                yield return new CompletionItem
                {
                    Kind = CompletionItemKind.Field,
                    InsertText = $"{missingField.FieldName}",
                    InsertTextFormat = InsertTextFormat.PlainText,
                    InsertTextMode = InsertTextMode.AsIs,
                    Label = missingField.FieldName,
                };
            }
        }
    }
}