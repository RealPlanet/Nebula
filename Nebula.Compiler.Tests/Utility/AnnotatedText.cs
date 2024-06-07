using Nebula.Commons.Text;
using System.Collections.Immutable;
using System.Text;

namespace Nebula.Tests.Utility
{
    public sealed class AnnotatedText
    {
        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Spans = spans;
            Text = text;
        }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            StringBuilder stringBuilder = new();
            Stack<int> starStack = new();
            ImmutableArray<TextSpan>.Builder? spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            int position = 0;

            foreach (char c in text)
            {
                switch (c)
                {
                    case '[':
                        starStack.Push(position);
                        break;
                    case ']':
                        if (starStack.Count == 0)
                        {
                            throw new ArgumentException("Unexpected '[' in text", nameof(text));
                        }

                        int start = starStack.Pop();
                        int end = position;
                        spanBuilder.Add(TextSpan.FromBounds(start, end));
                        break;
                    default:
                        position++;
                        stringBuilder.Append(c);
                        break;
                }
            }

            if (starStack.Count != 0)
            {
                throw new ArgumentException("Unclosed '[' in text", nameof(text));
            }

            return new AnnotatedText(stringBuilder.ToString(), spanBuilder.ToImmutable());
        }

        private static string Unindent(string text)
        {
            string[] lines = UnindentLines(text);
            return string.Join(Environment.NewLine, lines);
        }

        public static string[] UnindentLines(string text)
        {
            List<string> lines = new();
            using (StringReader stringReader = new(text))
            {
                string? line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            int minIndentation = int.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                string? line = lines[i];

                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                int indentation = line.Length - line.TrimStart().Length;
                minIndentation = Math.Min(minIndentation, indentation);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                lines[i] = lines[i][minIndentation..];
            }

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[^1].Length == 0)
                lines.RemoveAt(lines.Count - 1);
            return lines.ToArray();
        }
    }

}
