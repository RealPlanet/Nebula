using Nebula.Commons.Reporting;
using Nebula.Commons.Reporting.Strings;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Compiler.Tests.Utility;
using Nebula.Shared.Enumerators;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class ReportingTests
    {
        [TestMethod]
        public void VoidFunctionCantReturnValue()
        {
            const string text = @"
                func void test()
                {
                    return [1];
                }
            ";

            string template = BinderMessagesProvider.VoidFunctionCannotReturnValue.MessageTemplate;
            string diagnostics = string.Format(template, "test");

            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void FunctionWithReturnTypeCannotReturnVoid()
        {
            const string text = @"
                func int test()
                {
                    [return];
                }
            ";

            string diagnostics = string.Format(BinderMessagesProvider.FunctionExpectsReturn.MessageTemplate,
                "test",
                "int");

            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void FunctionNotAllPathsReturnValue()
        {
            const string text = @"
                [func bool test(int n)]
                {
                    if (n > 10)
                       return true;
                }
            ";

            string diagnostic = string.Format(BinderMessagesProvider.NotAllPathsReturn.MessageTemplate, "test");
            AssertDiagnostics(text, diagnostic);
        }

        [TestMethod]
        public void AllReportMessagesHaveUniqueCode()
        {
            EBinderMessages[] codes = Enum.GetValues<EBinderMessages>();
            IEnumerable<EBinderMessages> distinctCodes = codes.Distinct();
            Assert.AreEqual(codes.Length, distinctCodes.Count());
        }

        [TestMethod]
        public void ExpressionMustHaveValue()
        {
            const string text = @"
                func void test(int n)
                {
                    return;
                }
                
                func void main()
                {
                    const int value = [test(100)];
                }
            ";

            string diagnostics = BinderMessagesProvider.ExpressionMustHaveValue.MessageTemplate;
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void IfStatementReportsNotReachableCodeWarning()
        {
            string text = @"
                func void test()
                {
                    const int x = 4 * 3;
                    int testX = 0;
                    if (x > 12)
                    {
                        [testX = 12];
                    }
                    else
                    {
                        testX = 1;
                    }
                }
            ";

            string diagnostics = BinderMessagesProvider.UnreachableCodeDetected.MessageTemplate;
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void ElseStatementReportsNotReachableCodeWarning()
        {
            string text = @"
                func int test()
                {
                    if (true)
                    {
                        return 1;
                    }
                    else
                    {
                        [return] 0;
                    }
                }
            ";

            string diagnostics = BinderMessagesProvider.UnreachableCodeDetected.MessageTemplate;
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void WhileStatementReportsNotReachableCodeWarning()
        {
            string text = @"
                func void test()
                {
                    while (false)
                    {
                        [continue];
                    }
                }
            ";

            string diagnostics = BinderMessagesProvider.UnreachableCodeDetected.MessageTemplate;
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        [DataRow("func void main() { [break]; }", "break")]
        [DataRow("func void main() { [continue]; }", "continue")]
        public void InvalidBreakOrContinue(string text, string keyword)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.InvalidBreakOrContinue;
            string diagnostics = string.Format(template, keyword);
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void ParameterAlreadyDeclared()
        {
            const string text = @"
                func int sum(int a, int b, [int a])
                {
                    return a + b;
                }";

            (EBinderMessages code, string template) = BinderMessagesProvider.ParameterAlreadyDeclared;
            string diagnostics = string.Format(template, "a");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void WrongArgumentType()
        {
            const string text = @"
                func bool test(int n)
                {
                    return n > 10;
                }
                
                func void main()
                {
                    const string testValue = ""string"";
                    test([testValue]);
                }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "string", "int");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void BadType()
        {
            const string text = @"
                func void test([invalidtype] n)
                {
                }";

            (EParserMessages code, string template) = ParserMessagesProvider.TypeDoesNotExist;
            string diagnostics = string.Format(template, "invalidtype");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void VariableDeclarationReportsRedecleration()
        {
            const string text = @"
                func void test()
                {
                    int x = 0;
                    int y = 1;
                    {
                        int x = 10;
                    }
                    int [x] = 5;
                }";
            (EParserMessages code, string template) = ParserMessagesProvider.VariableAlreadyDeclared;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void InvokeFunctionArgumentsMissing()
        {
            const string text = @"
                func void main(){
                    s[()];
                }

                func void s(string s){}";

            (EBinderMessages code, string template) = BinderMessagesProvider.WrongNumberOfArguments;
            string diagnostics = string.Format(template, "s", 1, 0);
            AssertDiagnostics(text, diagnostics);

        }

        [TestMethod]
        public void InvokeFunctionArgumentsTooMany()
        {
            const string text = @"
                func void main(){
                    s(1[, 2 ,3]);
                }

                func void s(string s){}";

            (EBinderMessages code, string template) = BinderMessagesProvider.WrongNumberOfArguments;
            string diagnostics = string.Format(template, "s", 1, 3);
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void BlockStatementNoInfiniteLoop()
        {
            const string text = @"func void main()
                {
                {
                [[)]]
                }[]";

            const string report = @"
                        Unexpected token 'ClosedParenthesisToken', expected type 'IdentifierToken'.
                        Unexpected token 'ClosedParenthesisToken', expected type 'SemicolonToken'.
                        Unexpected token 'EndOfFileToken', expected type 'ClosedBracketToken'.
                          ";

            AssertDiagnostics(text, report);
        }

        [TestMethod]
        public void InvokeFunctionArgumentsNoInfiniteLoop()
        {
            const string text = @"func void main()
            {
                print(""Hi""=[)];
            }";

            const string diagnostics = @"
                Unexpected token 'ClosedParenthesisToken', expected type 'IdentifierToken'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void FunctionParametersNoInfiniteLoop()
        {
            const string text = @"
                func void hi(string name[[[=]]][[[[)]]]]
                {
                    print(""Hi "" + name + ""!"" );
                }[]";

            const string diagnostics = @"
                Unexpected token 'EqualsToken', expected type 'ClosedParenthesisToken'.
                Unexpected token 'EqualsToken', expected type 'OpenBracketToken'.
                Unexpected token 'EqualsToken', expected type 'IdentifierToken'.
                Unexpected token 'ClosedParenthesisToken', expected type 'IdentifierToken'.
                Unexpected token 'ClosedParenthesisToken', expected type 'SemicolonToken'.
                Unexpected token 'ClosedParenthesisToken', expected type 'IdentifierToken'.
                Unexpected token 'ClosedParenthesisToken', expected type 'SemicolonToken'.
                Unexpected token 'EndOfFileToken', expected type 'ClosedBracketToken'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void NameExpressionReportsNoErrorForInsertedToken()
        {
            const string text = "func void main() { 1 + [;] }";

            (EBinderMessages code, string template) = BinderMessagesProvider.UnexpectedToken;
            string diagnostics = string.Format(template, NodeType.SemicolonToken.ToString(), NodeType.IdentifierToken.ToString());
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void NameReportsUndefined()
        {
            const string text = "func void main() { int y = [x] * 10;}";
            (EBinderMessages code, string template) = BinderMessagesProvider.VariableDoesNotExists;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void AssignmentReportsUndefined()
        {
            const string text = "func void main() { [x] = 10; }";
            (EBinderMessages code, string template) = BinderMessagesProvider.VariableDoesNotExists;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void AssignmentExpressionReportsNotAVariable()
        {
            const string text = "func void main() { [print] = 42; } func void print(){}";

            (EBinderMessages code, string template) = BinderMessagesProvider.NameIsNotAVariable;
            string diagnostics = string.Format(template, "print");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void CompoundExpressionAssignmentNonDefinedVariableReportsUndefined()
        {
            const string text = "func void main() { [x] += 10; }";

            (EBinderMessages code, string template) = BinderMessagesProvider.VariableDoesNotExists;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void AssignmentReportsCannotAssign()
        {
            const string text = @"func int main()
                        {
                            const int x = 10;
                            x [=] 0;
                            return x;
                        }
                        ";
            (EBinderMessages code, string template) = BinderMessagesProvider.CannotReassignReadonlyVariable;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void CompoundDeclarationExpression_Reports_CannotAssign()
        {
            string? text = @"func void main()
                {
                    const int x = 10;
                    x [+=] 1;
                }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotReassignReadonlyVariable;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void AssignmentReportsCannotConvert()
        {
            const string text = @"func void main()
                        {
                            int x = 10;
                            x = [true];
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "bool", "int");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void UnaryReportsUndefined()
        {
            const string text = @"func void main()
                        {
                            [+]true;
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.UnaryOperatorNotDefined;
            string diagnostics = string.Format(template, "+", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void BinaryReportsUndefined()
        {
            const string text = @"func void main()
                        {
                            int a = 10 [+] true;
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.BinaryOperatorNotDefined;
            string diagnostics = string.Format(template, "+", "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void CompoundExpressionReportsUndefined()
        {
            string? text = @"func void main()
                        {
                            int x = 10;
                            x [+=] false;
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.BinaryOperatorNotDefined;
            string diagnostics = string.Format(template, "+=", "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void IfStatementReportsCannotConvert()
        {
            const string text = @"func void main()
                        {
                            int x = 0;
                            if ([10])
                                x = 10;
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void WhileStatementReportsCannotConvert()
        {
            const string text = @"func void main()
                        {
                            int x = 0;
                            while ([10])
                                x = 10;
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void DoWhileStatementReportsCannotConvert()
        {
            const string text = @"func void main()
                        {
                            int x = 0;
                            do
                                x = 10;
                            while ([10]);
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void CallExpressionReportsUndefined()
        {
            const string text = "func void main() { [foo](42); }";

            (EBinderMessages code, string template) = BinderMessagesProvider.FunctionDoesNotExists;
            string diagnostics = string.Format(template, "foo");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void CallExpressionReportsNotAFunction()
        {
            const string text = @"func void main()
                {
                    const int foo = 42;
                    [foo](42);
                }";

            (EBinderMessages code, string template) = BinderMessagesProvider.IdentifierIsNotAFunction;
            string diagnostics = string.Format(template, "foo");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void VariablesCanShadowFunctions()
        {
            const string text = @"
                native void print();
                func void main()
                {
                    const int print = 42;
                    [print](""test"");
                }";

            (EBinderMessages code, string template) = BinderMessagesProvider.IdentifierIsNotAFunction;
            string diagnostics = string.Format(template, "print");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void ForStatementReportsBinaryOperatorNotDefined()
        {
            const string text = @"func void main()
                        {
                            int result = 0;
                            for (bool i = false; i [!=] 10;) {}
                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.BinaryOperatorNotDefined;
            string diagnostics = string.Format(template, "!=", "bool", "int");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void ForStatementReportsCannotConvertUpperBound()
        {
            const string text = @"func void main()
                        {
                            int result = 0;
                            for (int i = 0; i [<] true; i+=1)
                                result = result + i;

                        }";

            (EBinderMessages code, string template) = BinderMessagesProvider.BinaryOperatorNotDefined;
            string diagnostics = string.Format(template, "<", "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertDiagnostics(string text, string reportText)
        {
            AnnotatedText? annotatedText = AnnotatedText.Parse(text);
            SourceCode sourceCode = SourceCode.From(annotatedText.Text, "test_text");
            Core.Compilation.Compiler.Compile(new() { EmitProgram = false, Sources = { sourceCode } }, out Core.Compilation.Compiler.Result result);
            Report finalReport = result.Report;

            string[]? expectedReport = AnnotatedText.UnindentLines(reportText);
            if (annotatedText.Spans.Length != expectedReport.Length)
            {
                throw new Exception("ERROR :: Must mark as many spans as there are expected reports");
            }

            finalReport.RemoveWarning(BinderMessagesProvider.NamespaceNotSet.Code.ToString());

            Assert.AreEqual(expectedReport.Length, finalReport.Count);

            int count = 0;
            foreach (ReportMessage message in finalReport)
            {
                string? expectedMessage = expectedReport[count];
                string? actualMessage = message.Message;

                TextSpan expectedSpan = annotatedText.Spans[count];
                TextSpan actualSpan = message.Location.Span;

                Assert.AreEqual(expectedMessage, actualMessage);
                Assert.AreEqual(expectedSpan, actualSpan);

                count++;
            }
        }
    }
}
