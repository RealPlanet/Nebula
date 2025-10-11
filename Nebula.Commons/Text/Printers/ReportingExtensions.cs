using Nebula.Commons.Reporting;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace Nebula.Commons.Text.Printers
{
    public static class ReportingExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
            {
                return !Console.IsOutputRedirected;
            }

            if (writer == Console.Error)
            {
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out
            }

            return writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole();
        }
        private static void SetForeground(this TextWriter writer, ConsoleColor consoleColor)
        {
            if (writer.IsConsole())
            {
                Console.ForegroundColor = consoleColor;
            }
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
            {
                Console.ResetColor();
            }
        }

        public static void WriteReport(this TextWriter writer, Report report)
        {
            foreach (ReportMessage msg in report.Where(d => d.Location.Text is null))
            {
                ConsoleColor messageColor = msg.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                writer.SetForeground(messageColor);
                writer.WriteLine(msg);
                writer.ResetColor();
            }

            foreach (ReportMessage msg in report.Where(d => d.Location.Text is not null)
                                    .OrderBy(d => d.Location.Text!.FileName)
                                    .ThenBy(d => d.Location.Span.Start)
                                    .ThenBy(d => d.Location.Span.Length))
            {
                WriteMessage(writer, msg);
            }
        }

        public static void WriteMessage(this TextWriter writer, ReportMessage msg)
        {
            SourceCode text = msg.Location.Text!;
            string fileName = msg.Location.FileName;
            int startLine = msg.Location.StartLine + 1;
            int startCharacter = msg.Location.StartCharacter + 1;
            int endLine = msg.Location.EndLine + 1;
            int endCharacter = msg.Location.EndCharacter + 1;

            TextSpan span = msg.Location.Span;
            int lineIndex = text.GetLineIndex(span.Start);
            int lineNumber = lineIndex + 1;
            TextLine? line = text.Lines[lineIndex];
            int character = span.Start - line.Start + 1;

            writer.WriteLine();
            ConsoleColor messageColor = msg.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
            writer.SetForeground(messageColor);
            writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
            writer.WriteLine(msg);
            writer.ResetColor();

            TextSpan prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
            TextSpan suffixSpan = TextSpan.FromBounds(span.End, line.End);

            string? prefix = text.ToString(prefixSpan);
            string? errorText = text.ToString(span);
            string? suffix = text.ToString(suffixSpan);

            writer.Write("    ");
            writer.Write(prefix);
            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write(errorText);
            writer.ResetColor();

            writer.WriteLine(suffix);
            writer.WriteLine();
        }
    }
}
