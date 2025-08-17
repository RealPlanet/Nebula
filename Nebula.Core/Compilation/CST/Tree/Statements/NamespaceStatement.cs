using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.CST.Tree.Statements
{
    public sealed class NamespaceStatement
        : Statement
    {
        public static NamespaceStatement Empty => new("");

        public override NodeType Type => NodeType.NamespaceDeclaration;
        public Token Keyword { get; }
        public Token Namespace { get; }
        public Token Semicolon { get; }

        public NamespaceStatement(SourceCode sourceCode, Token keyword, Token name, Token semicolon)
            : base(sourceCode)
        {
            Keyword = keyword;
            Namespace = name;
            Semicolon = semicolon;
        }

        /// <summary>
        /// Creates a namespace from a name (mocks all underlying structures)
        /// </summary>
        public NamespaceStatement(string name)
            : base(SourceCode.From($"namespace \"{name}\";", ""))
        {
            Keyword = new Token(SourceCode, NodeType.NamespaceKeyword, 0, SyntaxEx.GetText(NodeType.NamespaceKeyword), null, ImmutableArray<Trivia>.Empty, ImmutableArray<Trivia>.Empty);
            Namespace = new Token(SourceCode, NodeType.StringToken, 10, $"\"{name}\"", name, ImmutableArray<Trivia>.Empty, ImmutableArray<Trivia>.Empty);
            Semicolon = new Token(SourceCode, NodeType.SemicolonToken, 10 + Namespace.Text.Length + 1, ";", null, ImmutableArray<Trivia>.Empty, ImmutableArray<Trivia>.Empty);
        }

        public bool IsEmpty => this == Empty || Namespace.Text.Length - 2 == 0;

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return Namespace;
            yield return Semicolon;
        }
    }
}
