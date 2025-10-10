using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Compiler.Tests.Utility;
using Nebula.Core.Compilation;
using Nebula.Core.Compilation.CST.Parsing;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using System.Diagnostics;
using System.Text;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        [DynamicData(nameof(GetCanParseData))]
        public void Parser_CanParseCorrectly(NodeType expectedType, string text)
        {
            Expression a = ParseExpression(text);
            Assert.AreEqual(NodeType.LiteralExpression, a.Type);
            LiteralExpression litExp = (LiteralExpression)a;
            Assert.AreEqual(expectedType, litExp.Literal.Type);
        }

        [TestMethod]
        [DynamicData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedences(NodeType op1, NodeType op2)
        {
            int op1Precedence = op1.GetBinaryPrecedence();
            int op2Precedence = op2.GetBinaryPrecedence();

            string? op1Text = SyntaxEx.GetText(op1);
            string? op2Text = SyntaxEx.GetText(op2);

            string text = $"a {op1Text} b {op2Text} c;";
            Expression? expression = ParseExpression(text);

            Debug.Assert(op1Text != null);
            Debug.Assert(op2Text != null);

            if (op2 == NodeType.DotToken)
            {
                //       op1
                //      /   \
                //      a    objAccess
                //           /  \
                //          b    c

                using (AssertingEnum? Enum = new(expression))
                {
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "a");
                    Enum.AssertToken(op1, op1Text);
                    Enum.AssertNode(NodeType.ObjectVariableAccessExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "b");
                    Enum.AssertToken(op2, op2Text);
                    Enum.AssertToken(NodeType.IdentifierToken, "c");
                }

                return;
            }

            if (op1Precedence >= op2Precedence)
            {
                //       op2
                //      /   \
                //   op1    c
                //  /  \
                // a    b

                using (AssertingEnum? Enum = new(expression))
                {
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "a");
                    Enum.AssertToken(op1, op1Text);

                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "b");
                    Enum.AssertToken(op2, op2Text);

                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "c");
                }
            }
            else
            {
                //       op1
                //      /   \
                //      a    op2
                //           /  \
                //          b    c

                using (AssertingEnum? Enum = new(expression))
                {
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "a");
                    Enum.AssertToken(op1, op1Text);
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "b");
                    Enum.AssertToken(op2, op2Text);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "c");
                }
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedences(NodeType UnaryType, NodeType BinaryType)
        {
            int unaryPrecedence = UnaryType.GetUnaryPrecedence();
            int binaryPrecedence = BinaryType.GetBinaryPrecedence();

            string? unaryText = SyntaxEx.GetText(UnaryType);
            string? binaryText = SyntaxEx.GetText(BinaryType);

            string text = $"{unaryText} a {binaryText} b;";
            Expression? expression = ParseExpression(text);

            Debug.Assert(unaryText != null);
            Debug.Assert(binaryText != null);

            if (unaryPrecedence >= binaryPrecedence)
            {
                //   binary
                //   /    \
                // unary   b
                //  | 
                //  a 

                using (AssertingEnum? Enum = new(expression))
                {
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.UnaryExpression);
                    Enum.AssertToken(UnaryType, unaryText);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "a");
                    Enum.AssertToken(BinaryType, binaryText);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "b");
                }
            }
            else
            {
                //   unary
                //     |
                //   binary
                //  /     \
                // a       b

                using (AssertingEnum? Enum = new(expression))
                {
                    Enum.AssertNode(NodeType.UnaryExpression);
                    Enum.AssertToken(UnaryType, unaryText);
                    Enum.AssertNode(NodeType.BinaryExpression);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "a");
                    Enum.AssertToken(BinaryType, binaryText);
                    Enum.AssertNode(NodeType.NameExpression);
                    Enum.AssertToken(NodeType.IdentifierToken, "b");
                }
            }
        }

        private static Expression ParseExpression(string text)
        {
            StringBuilder sb = new();
            sb.AppendLine("func void main() {");
            sb.AppendLine(text);
            sb.AppendLine("}");


            SourceCode sourceCode = SourceCode.From(sb.ToString(), nameof(ParseExpression));
            CompilationUnit root = Parser.Parse(sourceCode, out Commons.Reporting.Report _);
            Assert.AreEqual(1, root.Functions.Count);
            FunctionDeclaration member = root.Functions[0];
            Assert.AreEqual(1, member.Body.Statements.Length);
            Assert.IsInstanceOfType<ExpressionStatement>(member.Body.Statements[0]);
            return ((ExpressionStatement)member.Body.Statements[0]).Expression;
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData
        {
            get
            {
                foreach (NodeType op1 in SyntaxEx.GetBinaryOperatorTypes())
                {
                    foreach (NodeType op2 in SyntaxEx.GetBinaryOperatorTypes())
                    {
                        yield return new object[] { op1, op2 };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData
        {
            get
            {
                foreach (NodeType unary in SyntaxEx.GetUnaryOperatorTypes())
                {
                    foreach (NodeType binary in SyntaxEx.GetBinaryOperatorTypes())
                    {
                        yield return new object[] { unary, binary };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetCanParseData
        {
            get
            {
                List<(NodeType, string)> dataList = new()
                {
                    (NodeType.NumberToken, "1"),
                    (NodeType.NumberToken, "125"),
                    (NodeType.NumberToken, "1.25f"),
                    (NodeType.NumberToken, ".25f"),
                };

                foreach ((NodeType, string) i in dataList)
                {
                    yield return new object[] { i.Item1, i.Item2 };
                }
            }
        }
    }
}

