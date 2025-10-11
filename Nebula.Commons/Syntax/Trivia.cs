using Nebula.Commons.Text;

namespace Nebula.Commons.Syntax
{
    /// <summary>
    /// Trivia is associated with a token, each token can have leading and trailing trivia. Trivia can be single or multiline comments, whitespaces, ecc..
    /// </summary>
    public sealed class Trivia
    {
        public SourceCode SyntaxTree { get; }
        public NodeType Type { get; }
        public int Position { get; }
        public string Text { get; }
        public TextSpan Span => new(Position, Text?.Length ?? 0);

        public Trivia(SourceCode syntaxTree, NodeType type, int position, string text)
        {
            SyntaxTree = syntaxTree;
            Type = type;
            Position = position;
            Text = text;
        }
    }
}
