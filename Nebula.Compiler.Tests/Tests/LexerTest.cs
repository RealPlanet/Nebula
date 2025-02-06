using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation;
using Nebula.Core.Parsing.Lexing;
using Nebula.Tests.Utility;
using System.Text;

namespace Nebula.Core.Lexing.Tests
{
    [TestClass]
    public class LexerTest
    {
        private static string ListToMessage(SortedSet<NodeType> untestedTokens)
        {
            StringBuilder sb = new();

            foreach (NodeType n in untestedTokens)
                sb.AppendLine(n.ToString());

            return sb.ToString();
        }

        [TestMethod]
        public void Lexer_Lexes_UnterminatedString()
        {
            string unterminatedString = $"\"Hello World";
            SourceCode source = SourceCode.From(unterminatedString, "");
            IReadOnlyList<Token> tokens = Lexer.ParseFrom(source, out Report? parseReport);
            Assert.AreEqual(2, tokens.Count, "Collection should only contain string and EOF token");
            Token token = tokens[0];

            Assert.AreEqual(NodeType.StringToken, token.Type);
            Assert.AreEqual(source.Text, token.Text, "Token text is not matching");
            Assert.AreEqual(1, parseReport.Count, "Unexpected report count");
            ReportMessage error = parseReport.Errors.First();
            Assert.AreEqual(new TextSpan(0, 1), error.Location.Span, "Message error location does not match!");

            string reportMessage = string.Format(ReportMessageProvider.ErrorUnterminatedStringLiteral.MessageTemplate, unterminatedString);

            Assert.AreEqual(reportMessage, error.Message);
        }

        [TestMethod]
        public void Lexer_Tests_AllTokens()
        {
            List<NodeType>? tokenTypes = Enum.GetValues(typeof(NodeType))
                                    .Cast<NodeType>()
                                    .Where(k => k.IsToken())
                                    .ToList();

            IEnumerable<NodeType>? testedTokens = GetTokens().Concat(GetSeparators()).Select(t => t.Type);

            SortedSet<NodeType> untestedTokens = new(tokenTypes);
            untestedTokens.Remove(NodeType.Error);
            untestedTokens.Remove(NodeType.EndOfFileToken);
            untestedTokens.ExceptWith(testedTokens);

            Assert.AreEqual(0, untestedTokens.Count, ListToMessage(untestedTokens));
        }


        [TestMethod]
        [DynamicData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(NodeType type, string text)
        {
            SourceCode code = SourceCode.From(text, "");
            Token[] tokens = Lexer.ParseFrom(code, out Report? report).ToArray();

            Assert.AreEqual(2, tokens.Length);

            Token? token = tokens[0];
            Assert.AreEqual(type, token.Type);
            Assert.AreEqual(text, token.Text);
        }

        [TestMethod]
        [DynamicData(nameof(GetSeparatorsData))]
        public void Lexer_Lexes_Separator(NodeType type, string text)
        {
            SourceCode code = SourceCode.From(text, "");
            Token[] tokens = Lexer.ParseFrom(code, out Report? report).ToArray();

            Assert.AreEqual(1, tokens.Length);
            Token? token = tokens[0];

            Assert.AreEqual(1, token.LeadingTrivia.Length);

            Trivia? trivia = token.LeadingTrivia[0];

            Assert.AreEqual(type, trivia.Type);
            Assert.AreEqual(text, trivia.Text);
        }


        [TestMethod]
        [DynamicData(nameof(GetTokensPairsData))]
        public void Lexer_Lexes_TokenPairs(NodeType Type1, string Text1, NodeType Type2, string Text2)
        {
            string Text = Text1 + Text2;
            IReadOnlyList<Token> lexedTokens = Lexer.ParseFrom(SourceCode.From(Text, ""), out Report _);

            Token[] tokens = [.. lexedTokens];

            int expectedTokens = 2;// Without end of file
            Assert.AreEqual(expectedTokens + 1, tokens.Length, "Number of tokens does not match");
            Assert.AreEqual(Type1, tokens[0].Type);
            Assert.AreEqual(Text1, tokens[0].Text);
            Assert.AreEqual(Type2, tokens[1].Type);
            Assert.AreEqual(Text2, tokens[1].Text);
        }

        [TestMethod]
        [DynamicData(nameof(GetTokensPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairs_WithSeparator(NodeType type1,
                                                         string text1,
                                                         NodeType separatorType,
                                                         string separatorText,
                                                         NodeType type2,
                                                         string text2)
        {
            string text = text1 + separatorText + text2;
            Token[] tokens = Lexer.ParseFrom(SourceCode.From(text, ""), out Report _).ToArray();

            Assert.AreEqual(3, tokens.Length, tokens.ToStringEx());
            Assert.AreEqual(type1, tokens[0].Type);
            Assert.AreEqual(text1, tokens[0].Text);

            Assert.AreEqual(1, tokens[0].TrailingTrivia.Length);

            Trivia? separator = tokens[0].TrailingTrivia[0];
            Assert.AreEqual(separatorType, separator.Type);
            Assert.AreEqual(separatorText, separator.Text);

            Assert.AreEqual(type2, tokens[1].Type);
            Assert.AreEqual(text2, tokens[1].Text);
        }

        [DataTestMethod]
        [DataRow("foo")]
        [DataRow("foo42")]
        [DataRow("foo_42")]
        [DataRow("_foo")]
        public void Lexer_Lexes_Identifiers(string name)
        {
            SourceCode code = SourceCode.From(name, "");
            Token[] tokens = Lexer.ParseFrom(code, out Report? report).ToArray();
            Assert.AreEqual(2, tokens.Length);
            Token token = tokens[0];

            Assert.AreEqual(NodeType.IdentifierToken, token.Type);
            Assert.AreEqual(name, token.Text);
        }


        #region Data properties

        public static IEnumerable<object[]> GetTokensData
        {
            get
            {
                foreach ((NodeType Type, string Text) Token in GetTokens())
                {
                    yield return new object[] { Token.Type, Token.Text };
                }
            }
        }

        public static IEnumerable<object[]> GetSeparatorsData
        {
            get
            {
                foreach ((NodeType Type, string Text) Token in GetSeparators())
                {
                    yield return new object[] { Token.Type, Token.Text };
                }
            }
        }

        public static IEnumerable<object[]> GetTokensPairsData
        {
            get
            {
                foreach ((NodeType Type1, string Text1, NodeType Type2, string Text2) in GetTokensPairs())
                {
                    yield return new object[] { Type1, Text1, Type2, Text2 };
                }
            }
        }

        public static IEnumerable<object[]> GetTokensPairsWithSeparatorData
        {
            get
            {
                foreach ((NodeType Type1, string Text1, NodeType SeparatorType, string SeparatorText, NodeType Type2, string Text2) in GetTokensPairsWithSeparators())
                {
                    yield return new object[] { Type1, Text1, SeparatorType, SeparatorText, Type2, Text2 };
                }
            }
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Returns true if the two token types require a separator between them to be lexed.
        /// For example + += does not make sense as ++=
        /// </summary>
        private static bool RequiresSeparator(NodeType type1, NodeType type2)
        {
            bool t1IsKeyword = type1.IsKeyword();
            bool t2IsKeyword = type2.IsKeyword();

            if (t1IsKeyword && t2IsKeyword)
            {
                return true;
            }

            if (type1 == NodeType.StringToken && type2 == NodeType.StringToken)
                return true;

            if (type1 == NodeType.NumberToken && type2 == NodeType.NumberToken)
                return true;

            if (type1 == NodeType.NumberToken && type2 == NodeType.DotToken)
                return true;

            if (type1 == NodeType.IdentifierToken && type2 == NodeType.IdentifierToken)
                return true;

            if (type1 == NodeType.BangToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.BangToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.EqualsToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.EqualsToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.LessToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.LessToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.GreaterToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.GreaterToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.PlusToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.PlusToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.MinusToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.MinusToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.StarToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.StarToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.AmpersandToken && type2 == NodeType.DoubleAmpersandToken)
                return true;

            if (type1 == NodeType.AmpersandToken && type2 == NodeType.AmpersandToken)
                return true;

            if (type1 == NodeType.AmpersandToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.AmpersandToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.AmpersandToken && type2 == NodeType.AmpersandEqualsToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.SlashEqualsToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.StarEqualsToken)
                return true;

            if (type1 == NodeType.PipeToken && type2 == NodeType.DoublePipeToken)
                return true;

            if (type1 == NodeType.PipeToken && type2 == NodeType.PipeToken)
                return true;

            if (type1 == NodeType.PipeToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.PipeToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.PipeToken && type2 == NodeType.PipeEqualsToken)
                return true;

            if (type1 == NodeType.HatToken && type2 == NodeType.EqualsToken)
                return true;

            if (type1 == NodeType.HatToken && type2 == NodeType.DoubleEqualsToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.SlashToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.StarToken)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.SingleLineCommentTrivia)
                return true;

            if (type1 == NodeType.SlashToken && type2 == NodeType.MultiLineCommentTrivia)
                return true;

            if (type1 == NodeType.IdentifierToken && type2 == NodeType.NumberToken)
                return true;

            if (t1IsKeyword && type2 == NodeType.NumberToken)
                return true;

            if (t1IsKeyword && type2 == NodeType.IdentifierToken)
                return true;

            if (t2IsKeyword && type1 == NodeType.IdentifierToken)
                return true;

            return false;
        }

        private static IEnumerable<(NodeType Type1, string Text1, NodeType Type2, string Text2)> GetTokensPairs()
        {
            foreach ((NodeType Type, string Text) Token1 in GetTokens())
            {
                foreach ((NodeType Type, string Text) Token2 in GetTokens())
                {
                    if (!RequiresSeparator(Token1.Type, Token2.Type))
                    {
                        yield return (Token1.Type, Token1.Text, Token2.Type, Token2.Text);
                    }
                }
            }
        }

        private static IEnumerable<(NodeType Type1, string Text1,
                                    NodeType SeparatorType, string SeparatorText,
                                    NodeType Type2, string Text2)> GetTokensPairsWithSeparators()
        {
            foreach ((NodeType Type, string Text) t1 in GetTokens())
            {
                foreach ((NodeType Type, string Text) t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.Type, t2.Type))
                    {
                        foreach ((NodeType Type, string Text) s in GetSeparators())
                        {
                            if (!RequiresSeparator(t1.Type, s.Type) && !RequiresSeparator(s.Type, t2.Type))
                                yield return (t1.Type, t1.Text, s.Type, s.Text, t2.Type, t2.Text);
                        }
                    }
                }
            }
        }

        private static IEnumerable<(NodeType Type, string Text)> GetTokens()
        {
            IEnumerable<(NodeType tokType, string text)>? fixedTokens = Enum.GetValues(typeof(NodeType))
                                    .Cast<NodeType>()
                                    .Select(k => (tokType: k, text: SyntaxEx.GetText(k)))
                                    .Where(t => t.text is not null)
                                    .Cast<(NodeType tokType, string text)>();

            (NodeType, string)[]? dynamicTokens =
            [
                (NodeType.IdentifierToken, "a"),
                (NodeType.IdentifierToken, "abc"),
                (NodeType.NumberToken, "1"),
                (NodeType.NumberToken, "123"),
                (NodeType.NumberToken, ".1f"),
                (NodeType.NumberToken, "15.1f"),
                (NodeType.NumberToken, "123"),
                (NodeType.StringToken, "\"abc\""),
                (NodeType.StringToken, "\"ab\"\"c\""),
            ];

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<(NodeType Type, string Text)> GetSeparators()
            =>
        [
                (NodeType.WhiteSpaceTrivia, " "),
                (NodeType.WhiteSpaceTrivia, "    "),
                (NodeType.LinebreakTrivia, "\r"),
                (NodeType.LinebreakTrivia, "\n"),
                (NodeType.LinebreakTrivia, "\r\n"),
                //(NodeType.SingleLineCommentTrivia, "// abcd"), Not included intentionally
                (NodeType.MultiLineCommentTrivia, "/**/"),
        ];


        #endregion
    }
}
