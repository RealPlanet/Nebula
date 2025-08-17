using Nebula.Commons.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nebula.Commons.Syntax
{
    public abstract class Node
    {
        public abstract NodeType Type { get; }

        /// <summary>
        /// Returns the TextSpan of a node, including their children, for example this can be the length of a for loop (initialization + body)
        /// </summary>
        public virtual TextSpan Span
        {
            get
            {
                TextSpan first = GetChildren().First().Span;
                TextSpan last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }
        /// <summary>
        /// Returns the FULL TextSpan of a node, including their children, this includes the length of the normal text span plus the length of all the trivia
        /// </summary>
        public virtual TextSpan FullSpan
        {
            get
            {
                TextSpan first = GetChildren().First().FullSpan;
                TextSpan last = GetChildren().Last().FullSpan;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }
        public TextLocation Location => new(SourceCode, Span);
        public SourceCode SourceCode { get; }

        protected Node(SourceCode sourceCode)
        {
            SourceCode = sourceCode;
        }

        public abstract IEnumerable<Node> GetChildren();

        #region Writer
        public void WriteTo(TextWriter writer) => PrintTree(writer, this);

        private static void PrintTree(TextWriter textWriter, Node node, string indent = "", bool isLast = true)
        {
            bool toConsoleOutput = textWriter == Console.Out;
            Token? token = node as Token;

            if (toConsoleOutput)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            if (token is not null)
            {
                foreach (Trivia? trivia in token.LeadingTrivia)
                {
                    if (toConsoleOutput)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }

                    textWriter.Write(indent);
                    textWriter.Write("├──");

                    if (toConsoleOutput)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }

                    textWriter.WriteLine($"L: {trivia.Type}");
                }
            }

            bool hasTrailingTrivia = token is not null && token.TrailingTrivia.Any();
            string tokenMarker = !hasTrailingTrivia && isLast ? "└──" : "├──";

            if (toConsoleOutput)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            textWriter.Write(indent);
            textWriter.Write(tokenMarker);

            if (toConsoleOutput)
            {
                Console.ForegroundColor = node is Token ? ConsoleColor.Blue : ConsoleColor.Cyan;
            }

            textWriter.Write(node.Type);

            if (token is not null && token.Value is not null)
            {
                textWriter.Write($" {token.Value}");
            }

            if (toConsoleOutput)
            {
                Console.ResetColor();
            }

            textWriter.WriteLine();

            if (token is not null)
            {
                foreach (Trivia? trivia in token.TrailingTrivia)
                {
                    bool isLastTrailingTrivia = trivia == token.TrailingTrivia.Last();
                    string triviaMarker = isLast && isLastTrailingTrivia ? "└──" : "├──";

                    if (toConsoleOutput)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }

                    textWriter.Write(indent);
                    textWriter.Write(triviaMarker);

                    if (toConsoleOutput)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }

                    textWriter.WriteLine($"T: {trivia.Type}");
                }
            }

            indent += isLast ? "   " : "│  ";

            Node? lastChild = node.GetChildren().LastOrDefault();
            foreach (Node? Child in node.GetChildren())
            {
                PrintTree(textWriter, Child, indent, Child == lastChild);
            }
        }

        #endregion
    }
}
