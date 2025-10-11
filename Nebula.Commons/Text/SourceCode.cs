using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Nebula.Commons.Text
{
    public sealed class SourceCode
    {
        public ImmutableArray<TextLine> Lines { get; }
        public string Text { get; }
        public string FileName { get; }

        private SourceCode(string text, string fileName)
        {
            Lines = ParseLines(this, text);
            Text = text;
            FileName = fileName;
        }

        [DebuggerStepThrough]
        public static SourceCode From(string text, string fileName = "") => new(text, fileName);
        [DebuggerStepThrough]
        public static SourceCode From(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException(null, nameof(fileName));
            }

            return From(File.ReadAllText(fileName), fileName);
        }

        public char this[int index] => Text[index];
        public int Length => Text.Length;

        public int GetLineIndex(int position)
        {
            int lower = 0, upper = Lines.Length - 1;
            while (lower <= upper)
            {
                int index = lower + (upper - lower) / 2;
                int start = Lines[index].Start;

                if (start == position)
                {
                    return index;
                }

                if (start > position)
                {
                    upper = index - 1;
                    continue;
                }

                lower = index + 1;
            }

            return lower - 1;
        }

        private static ImmutableArray<TextLine> ParseLines(SourceCode sourceText, string text)
        {
            ImmutableArray<TextLine>.Builder? result = ImmutableArray.CreateBuilder<TextLine>();
            int lineStart = 0, position = 0;

            while (position < text.Length)
            {
                int lineBreakWidth = GetLineBreakWidth(text, position);
                if (lineBreakWidth == 0)
                {
                    position++;
                    continue;
                }

                AddLine(result, sourceText, position, lineStart, lineBreakWidth);
                position += lineBreakWidth;
                lineStart = position;
            }

            if (position >= lineStart)
            {
                AddLine(result, sourceText, position, lineStart, 0);
            }

            return result.ToImmutable();
        }

        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceCode sourceText, int position, int lineStart, int lineBreakWidth)
        {
            int lineLength = position - lineStart;
            int lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
            TextLine? line = new(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
            result.Add(line);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            char c = text[position];
            char l = position + 1 >= text.Length ? '\0' : text[position + 1];
            if (c == '\r' && l == '\n')
            {
                return 2;
            }

            if (c == '\r' || l == '\n')
            {
                return 1;
            }

            return 0;
        }

        public override string ToString() => Text;

        public string ToString(int start, int length) => Text.Substring(start, length);

        public string ToString(TextSpan span) => ToString(span.Start, span.Length);

        public string ToMD5Hash()
        {
            return BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(Text))).Replace("-", "").ToLowerInvariant();
        }
    }
}
