using Nebula.Commons.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Commons.Syntax
{
    public class Token
        : Node
    {
        public string Text { get; }
        public int TextPosition { get; }
        public override TextSpan Span => new(TextPosition, Text.Length);
        public override TextSpan FullSpan => new(TextPosition, Text.Length);
        public override NodeType Type { get; }
        public object? Value { get; }

        public bool IsMissing { get; }
        public ImmutableArray<Trivia> LeadingTrivia { get; }
        public ImmutableArray<Trivia> TrailingTrivia { get; }

        public Token(SourceCode sourceCode, NodeType type, int textPosition, string? text, object? value, ImmutableArray<Trivia> leadingTrivia, ImmutableArray<Trivia> trailingTrivia)
            : base(sourceCode)
        {
            Type = type;
            Text = text ?? string.Empty;
            IsMissing = text is null;
            TextPosition = textPosition;
            Value = value;
            LeadingTrivia = leadingTrivia;
            TrailingTrivia = trailingTrivia;
        }

        public override IEnumerable<Node> GetChildren() => Array.Empty<Node>();

        public override string ToString() => Type.ToString();
    }
}
