using Nebula.LSP.Documents;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class LSPDocumentTests
    {
        [TestMethod]
        public void ApplyChangeCorrectlyModifiesText()
        {
            NebulaDocument docu = new(string.Empty, new Uri(Environment.CurrentDirectory), 0, "HELLO WORLD!");
            docu.ApplyChange(new(0, 4), new(0, 10), ", World");

            Assert.AreEqual("HELLO, World!", docu.Text.ToString());
        }

        [TestMethod]
        public void ApplyChangeCorrectlyModifiesTextMultiLine()
        {
            NebulaDocument docu = new(string.Empty, new Uri(Environment.CurrentDirectory), 0, "HELLO\nWORLD!");
            docu.ApplyChange(new(0, 4), new(1, 6), ", World");

            Assert.AreEqual("HELLO, World!", docu.Text.ToString());
        }
    }
}
