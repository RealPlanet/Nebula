namespace Nebula.Commons.Text
{
    public readonly struct TextLocation
    {
        public SourceCode? Text { get; }
        public TextSpan Span { get; }
        public string FileName => Text!.FileName;
        public int StartLine => Text!.GetLineIndex(Span.Start);
        public int StartCharacter => Span.Start - Text!.Lines[StartLine].Start;
        public int EndLine => Text!.GetLineIndex(Span.End);
        public int EndCharacter => Span.End - Text!.Lines[StartLine].Start;

        public TextLocation(SourceCode text, TextSpan span)
        {
            Text = text;
            Span = span;
        }

        public TextLocation(SourceCode text)
            : this(text, TextSpan.Empty)
        {

        }

        public TextLocation()
            : this(null!, TextSpan.Empty)
        {

        }
    }
}