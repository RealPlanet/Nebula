using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation;
using Nebula.Core.Compilation.CST.Tree;
using Nebula.Core.Compilation.CST.Tree.Declaration;
using Nebula.Core.Compilation.CST.Tree.Declaration.Bundle;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Core.Compilation.CST.Tree.Statements;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.LSP
{
    public static class Extensions
    {
        public static Range ToRange(this TextLocation location)
        {
            return new(location.StartLine, location.StartCharacter, location.EndLine, location.EndCharacter);
        }

        public static DocumentSymbol ToSymbol(this CompilationUnit tree)
        {
            return tree.NamespaceStatement.ToSymbol(tree);
        }

        public static DocumentSymbol ToSymbol(this VariableDeclaration declaration)
        {
            return new DocumentSymbol
            {
                Kind = SymbolKind.Variable,
                Name = declaration.Identifier.Text,
                Range = declaration.Location.ToRange(),
                SelectionRange = declaration.Location.ToRange(),
            };
        }

        public static DocumentSymbol[] ToSymbol(this BinaryExpression declaration)
        {
            var symbols = new List<DocumentSymbol>();

            symbols.Add(new DocumentSymbol
            {
                Kind = SymbolKind.Operator,
                Name = declaration.Operator.Text,
                Range = declaration.Operator.Location.ToRange(),
                SelectionRange = declaration.Operator.Location.ToRange(),
            });

            return symbols.ToArray();
        }

        public static DocumentSymbol[] ToSymbol(this UnaryExpression declaration)
        {
            var symbols = new List<DocumentSymbol>();

            symbols.Add(new DocumentSymbol
            {
                Kind = SymbolKind.Operator,
                Name = declaration.Operator.Text,
                Range = declaration.Operator.Location.ToRange(),
                SelectionRange = declaration.Operator.Location.ToRange(),
            });

            return symbols.ToArray();
        }

        public static DocumentSymbol[] ToSymbol(this VariableDeclarationCollection declaration)
        {
            var children = new List<DocumentSymbol>();

            foreach (VariableDeclaration c in declaration.Declarations)
            {
                children.Add(c.ToSymbol());
            }

            return children.ToArray();
        }

        public static DocumentSymbol ToSymbol(this NamespaceStatement declaration, CompilationUnit tree)
        {
            TextLine lastLine = declaration.SourceCode.Lines.Last();
            List<DocumentSymbol> symbols = [];

            foreach (BundleDeclaration bundle in tree.Bundles)
            {
                var children = new List<DocumentSymbol>();
                foreach (BundleFieldDeclaration field in bundle.Fields)
                {
                    children.Add(new DocumentSymbol
                    {
                        Kind = SymbolKind.Field,
                        Name = field.Identifier.Text,
                        Range = field.Location.ToRange(),
                        SelectionRange = field.Identifier.Location.ToRange(),
                        Detail = string.Empty,
                    });
                }

                var bundleSymbol = new DocumentSymbol
                {
                    Kind = SymbolKind.Class,
                    Name = bundle.Name.Text,
                    Range = bundle.Location.ToRange(),
                    Children = children,
                    SelectionRange = bundle.Name.Location.ToRange(),
                    Detail = string.Empty
                };

                symbols.Add(bundleSymbol);
            }

            foreach (FunctionDeclaration node in tree.Functions)
            {
                var children = new List<DocumentSymbol>();

                foreach (Parameter par in node.Parameters)
                {
                    children.Add(new DocumentSymbol
                    {
                        Kind = SymbolKind.Variable,
                        Name = par.Identifier.Text,
                        Range = par.Location.ToRange(),
                        SelectionRange = par.Location.ToRange(),
                    });
                }

                foreach (Node child in node.Body.GetChildren())
                {
                    switch (child.Type)
                    {
                        case Commons.Syntax.NodeType.VariableDeclaration:
                            {
                                var vDec = (VariableDeclaration)child;
                                children.Add(vDec.ToSymbol());
                                break;
                            }
                        case Commons.Syntax.NodeType.VariableDeclarationCollection:
                            {
                                var vDec = (VariableDeclarationCollection)child;
                                children.AddRange(vDec.ToSymbol());
                                break;
                            }
                        case Commons.Syntax.NodeType.BinaryExpression:
                            {
                                var expr = (BinaryExpression)child;
                                children.AddRange(expr.ToSymbol());
                                break;
                            }
                        case Commons.Syntax.NodeType.UnaryExpression:
                            {
                                var expr = (UnaryExpression)child;
                                children.AddRange(expr.ToSymbol());
                                break;
                            }
                    }
                }

                symbols.Add(new DocumentSymbol
                {
                    Kind = SymbolKind.Function,
                    Name = node.Name.Text,
                    Range = node.Location.ToRange(),
                    SelectionRange = node.SignatureLocation.ToRange(),
                    Children = children,
                });
            }

            foreach(NativeFunctionDeclaration node in tree.NativeFunctions)
            {
                symbols.Add(new DocumentSymbol
                {
                    Kind = SymbolKind.Method,
                    Name = node.Name.Text,
                    Range = node.Location.ToRange(),
                    SelectionRange = node.SignatureLocation.ToRange(),
                });
            }

            return new DocumentSymbol
            {
                Kind = SymbolKind.Namespace,
                Name = declaration.Namespace.Text,
                Range = new Range(0, 0, declaration.SourceCode.Lines.Length, lastLine.End),
                SelectionRange = declaration.Location.ToRange(),
                Children = symbols,
            };
        }
    }
}
