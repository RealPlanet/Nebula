using Nebula.Core.Compilation.CST.Lexing;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class SyntaxExTests
    {
        [TestMethod]
        [DynamicData(nameof(GetSyntaxTypeData))]
        public void SyntaxFact_GetText_RoundTrips(NodeType Type)
        {
            string? txt = SyntaxEx.GetText(Type);
            if (txt is null)
            {
                return;
            }

            IReadOnlyList<Token> tokens = Lexer.ParseFrom(SourceCode.From(txt, string.Empty), out Report _);
            Assert.AreEqual(2, tokens.Count);

            Token tok = tokens[0];
            Assert.AreEqual(Type, tok.Type);
            Assert.AreEqual(txt, tok.Text);
        }

        public static IEnumerable<object[]> GetSyntaxTypeData
        {
            get
            {
                NodeType[]? Types = (NodeType[])Enum.GetValues(typeof(NodeType));
                foreach (NodeType v in Types)
                {
                    yield return new object[] { v };
                }
            }
        }
    }
}