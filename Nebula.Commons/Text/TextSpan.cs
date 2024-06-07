namespace Nebula.Commons.Text
{
    /// <summary>
    /// Rapresents a span of text in a source file.
    /// </summary>
    public readonly struct TextSpan
    {
        public static TextSpan Empty => new(0, 0);

        public int Start { get; }
        public int Length { get; }
        public readonly int End => Start + Length;

        public static TextSpan FromBounds(int start, int end) => new(start, end - start);

        /// <summary>
        /// Construct a TextSpawn providing the start and end positions of the span.
        /// </summary>
        /// <param name="start">Index of character in source file</param>
        /// <param name="length">>Index of character in source file</param>
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public override string ToString() => $"{Start}..{End}";

        public readonly bool OverlapsWith(TextSpan other) => Start < other.End && End > other.Start;
    }

}