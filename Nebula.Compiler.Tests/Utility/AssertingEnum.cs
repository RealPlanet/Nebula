using Nebula.Commons.Syntax;

namespace Nebula.Tests.Utility
{

    internal sealed class AssertingEnum : IDisposable
    {
        private readonly IEnumerator<Node> Enumerator;
        private bool HasError = false;

        private bool SetErrorState()
        {
            HasError = true;
            return false;
        }

        public AssertingEnum(Node Node)
        {
            Enumerator = Flatten(Node).GetEnumerator();
        }

        private static IEnumerable<Node> Flatten(Node Node)
        {
            Stack<Node>? NodeStack = new();
            NodeStack.Push(Node);

            while (NodeStack.Count > 0)
            {
                Node? CurrentNode = NodeStack.Pop();
                yield return CurrentNode;

                foreach (Node? Child in CurrentNode.GetChildren().Reverse())
                {
                    NodeStack.Push(Child);
                }
            }
        }

        public void AssertToken(NodeType TokType, string Text)
        {
            try
            {
                Assert.IsTrue(Enumerator.MoveNext());
                Assert.IsInstanceOfType<Token>(Enumerator.Current);
                Token token = (Token)Enumerator.Current;
                Assert.AreEqual(Text, token.Text);
                Assert.AreEqual(TokType, token.Type);
            }
            catch when (SetErrorState())
            {
                throw;
            }
        }

        public void AssertNode(NodeType NodeType)
        {
            try
            {
                Assert.IsTrue(Enumerator.MoveNext());
                Assert.AreEqual(NodeType, Enumerator.Current.Type);
                Assert.IsNotInstanceOfType<Token>(Enumerator.Current);
            }
            catch when (SetErrorState())
            {
                throw;
            }
        }

        public void Dispose()
        {
            if (!HasError)
            {
                Assert.IsFalse(Enumerator.MoveNext());
            }

            Enumerator.Dispose();
        }
    }

}
