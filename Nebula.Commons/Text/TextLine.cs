namespace Nebula.Commons.Text
{
    public sealed class TextLine
    {
        public SourceCode Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LengthIncludingLineBreak { get; }
        public TextSpan Span => new(Start, Length);
        public TextSpan SpanIncludingLineBreak => new(Start, LengthIncludingLineBreak);

        public TextLine(SourceCode text, int start, int length, int lengthIncludingLineBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            LengthIncludingLineBreak = lengthIncludingLineBreak;
        }

        public override string ToString() => Text.ToString(Span);
    }
}
