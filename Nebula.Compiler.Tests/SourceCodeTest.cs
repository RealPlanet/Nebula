using Nebula.Commons.Text;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class SourceCodeTest
    {
        [DataTestMethod]
        [DataRow(".", 1)]
        [DataRow(".\n", 2)]
        [DataRow(".\r\n", 2)]
        [DataRow(".\r\n\r\n", 3)]

        public void SourceTestCorrectlyCountsLines(string text, int expectedLineCount)
        {
            SourceCode source = SourceCode.From(text, "");
            Assert.AreEqual(expectedLineCount, source.Lines.Length);
        }
    }
}