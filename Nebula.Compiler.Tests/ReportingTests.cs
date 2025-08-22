using Nebula.Compiler.Tests.Utility;

namespace Nebula.Compiler.Tests
{
    [TestClass]
    public class ReportingTests
    {
        [TestMethod]
        public void AllMessagesFromProviderAreUnique()
        {
            System.Reflection.PropertyInfo[] allProperties = typeof(ReportMessageProvider).GetProperties();

            IEnumerable<System.Reflection.PropertyInfo> allErrors = allProperties.Where(p => p.Name.StartsWith("Error"));
            IEnumerable<System.Reflection.PropertyInfo> allWarning = allProperties.Where(p => p.Name.StartsWith("Warning"));
            IEnumerable<System.Reflection.PropertyInfo> allInfo = allProperties.Where(p => p.Name.StartsWith("Info"));

            Assert.AreEqual(allProperties.Length, allErrors.Count() + allWarning.Count() + allInfo.Count());

            HashSet<int> encounteredErrors = [];
            foreach (System.Reflection.PropertyInfo? tuple in allInfo)
            {
                object? value = tuple.GetValue(null);
                Assert.IsNotNull(value);

                (ReportMessageCodes Code, string MessageTemplate) = ((ReportMessageCodes Code, string MessageTemplate))value;
                Assert.IsFalse(encounteredErrors.TryGetValue((int)Code, out int other), $"Duplicate code detected: {Code}");

                encounteredErrors.Add((int)Code);
            }

            foreach (System.Reflection.PropertyInfo? tuple in allErrors)
            {
                object? value = tuple.GetValue(null);
                Assert.IsNotNull(value);

                (ReportMessageCodes Code, string MessageTemplate) = ((ReportMessageCodes Code, string MessageTemplate))value;
                Assert.IsFalse(encounteredErrors.TryGetValue((int)Code, out int other), $"Duplicate code detected: {Code}");

                encounteredErrors.Add((int)Code);
            }

            foreach (System.Reflection.PropertyInfo? tuple in allWarning)
            {
                object? value = tuple.GetValue(null);
                Assert.IsNotNull(value);

                (ReportMessageCodes Code, string MessageTemplate) = ((ReportMessageCodes Code, string MessageTemplate))value;
                Assert.IsFalse(encounteredErrors.TryGetValue((int)Code, out int other), $"Duplicate code detected: {Code}");

                encounteredErrors.Add((int)Code);
            }
        }

        [TestMethod]
        public void VoidFunctionCantReturnValue()
        {
            const string text = @"
                func void test()
                {
                    return [1];
                }
            ";

            string template = ReportMessageProvider.ErrorVoidFunctionCannotReturnValue.MessageTemplate;
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

            string diagnostics = string.Format(ReportMessageProvider.ErrorFunctionExpectsReturn.MessageTemplate,
                "test",
                "int");

            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void FunctionNotAllPathsReturnValue()
        {
            const string text = @"
                func bool [test](int n)
                {
                    if (n > 10)
                       return true;
                }
            ";

            string diagnostic = string.Format(ReportMessageProvider.ErrorNotAllPathsReturn.MessageTemplate, "test");
            AssertDiagnostics(text, diagnostic);
        }

        [TestMethod]
        public void AllReportMessagesHaveUniqueCode()
        {
            ReportMessageCodes[] codes = Enum.GetValues<ReportMessageCodes>();
            IEnumerable<ReportMessageCodes> distinctCodes = codes.Distinct();
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

            string diagnostics = ReportMessageProvider.ErrorExpressionMustHaveValue.MessageTemplate;
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

            string diagnostics = ReportMessageProvider.WarningUnreachableCodeDetected.MessageTemplate;
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

            string diagnostics = @"
                Unreachable code detected.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        //[TestMethod]
        //public void Evaluator_WhileStatement_Reports_NotReachableCode_Warning()
        //{
        //    string text = @"
        //        fn test()
        //        {
        //            while false
        //            {
        //                [continue]
        //            }
        //        }
        //    ";

        //    string diagnostics = @"
        //        Unreachable code detected.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        ////[Theory]
        ////[InlineData("[break]", "break")]
        ////[InlineData("[continue]", "continue")]
        ////public void Evaluator_Invalid_Break_Or_Continue(string text, string keyword)
        ////{
        ////    string? diagnostics = $@"
        ////        The keyword <{keyword}> can only be used inside loops.
        ////    ";
        ////
        ////    AssertDiagnostics(text, diagnostics);
        ////}

        [TestMethod]
        public void ParameterAlreadyDeclared()
        {
            const string text = @"
                func int sum(int a, int b, [int a])
                {
                    return a + b;
                }
            ";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorParameterAlreadyDeclared;
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
                }
            ";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotConvertTypeImplicity;
            string diagnostics = string.Format(template, "string", "int");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void Evaluator_Bad_Type()
        {
            const string text = @"
                func void test([invalidtype] n)
                {
                }
            ";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorTypeDoesNotExist;
            string diagnostics = string.Format(template, "invalidtype");
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void Evaluator_VariableDeclaration_Reports_Redecleration()
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
                }
                ";
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorVariableAlreadyDeclared;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        //[TestMethod]
        //public void Evaluator_InvokeFunctionArguments_Missing()
        //{
        //    const string text = @"
        //        print([)]
        //    ";

        //    const string diagnostics = @"
        //        Function <print> requires 1 arguments but was given 0.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_InvokeFunctionArguments_Exceeding()
        //{
        //    const string text = @"
        //        print(""Hello""[, "" "", "" world!""])
        //    ";

        //    const string diagnostics = @"
        //        Function <print> requires 1 arguments but was given 3.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_BlockStatement_NoInfiniteLoop()
        //{
        //    const string text = @"
        //        {
        //        [)][]
        //        ";

        //    const string report = @"
        //                Unexpected token:<CLOSE_PARENTHESIS_TOKEN>, expected <IDENTIFIER_TOKEN>.
        //                Unexpected token:<END_OF_FILE_TOKEN>, expected <CLOSE_BRACE_TOKEN>.
        //                  ";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        //{
        //    const string text = @"
        //        print(""Hi""[[=]][)]
        //    ";

        //    const string diagnostics = @"
        //        Unexpected token:<EQUALS_TOKEN>, expected <CLOSE_PARENTHESIS_TOKEN>.
        //        Unexpected token:<EQUALS_TOKEN>, expected <IDENTIFIER_TOKEN>.
        //        Unexpected token:<CLOSE_PARENTHESIS_TOKEN>, expected <IDENTIFIER_TOKEN>.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_FunctionParameters_NoInfiniteLoop()
        //{
        //    const string text = @"
        //        fn hi(string name[[[=]]][)]
        //        {
        //            print(""Hi "" + name + ""!"" )
        //        }[]
        //    ";

        //    const string diagnostics = @"
        //        Unexpected token:<EQUALS_TOKEN>, expected <CLOSE_PARENTHESIS_TOKEN>.
        //        Unexpected token:<EQUALS_TOKEN>, expected <OPEN_BRACE_TOKEN>.
        //        Unexpected token:<EQUALS_TOKEN>, expected <IDENTIFIER_TOKEN>.
        //        Unexpected token:<CLOSE_PARENTHESIS_TOKEN>, expected <IDENTIFIER_TOKEN>.
        //        Unexpected token:<END_OF_FILE_TOKEN>, expected <CLOSE_BRACE_TOKEN>.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        [TestMethod]
        public void NameExpressionReportsNoErrorForInsertedToken()
        {
            const string text = "func void main() { 1 + [;] }";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorUnexpectedToken;
            string diagnostics = string.Format(template, NodeType.SemicolonToken.ToString(), NodeType.IdentifierToken.ToString());
            AssertDiagnostics(text, diagnostics);
        }

        [TestMethod]
        public void Evaluator_Name_Reports_Undefined()
        {
            const string text = "func void main() { int y = [x] * 10;}";
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorVariableDoesNotExists;
            string diagnostics = string.Format(template, "x");
            AssertDiagnostics(text, diagnostics);
        }

        //[TestMethod]
        //public void Evaluator_Assignment_Reports_Undefined()
        //{
        //    const string text = "[x] = 10";
        //    const string report = "Variable name does not exist <x>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        //{
        //    const string text = "[print] = 42";

        //    const string diagnostics = @"
        //        <print> is not a variable.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_CompoundExpression_Assignment_NonDefinedVariable_Reports_Undefined()
        //{
        //    const string text = "[x] += 10";

        //    const string diagnostics = "Variable name does not exist <x>.";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_Assignment_Reports_CannotAssign()
        //{
        //    const string text = @"
        //                {
        //                    const x = 10
        //                    x [=] 0
        //                    return x
        //                }
        //                ";
        //    const string report = "Cannot reassign value of read-only variable <x>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_CompoundDeclarationExpression_Reports_CannotAssign()
        //{
        //    string? text = @"
        //        {
        //            const x = 10
        //            x [+=] 1
        //        }
        //    ";

        //    string? diagnostics = @"
        //        Cannot reassign value of read-only variable <x>.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_Assignment_Reports_CannotConvert()
        //{
        //    const string text = @"
        //                {
        //                    var x = 10
        //                    x = [true]
        //                }
        //                ";

        //    const string report = "Cannot convert type of <bool> to <int>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_Unary_Reports_Undefined()
        //{
        //    const string text = "[+]true";

        //    const string report = "Unary operator <+> is not defined for type <bool>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_Binary_Reports_Undefined()
        //{
        //    const string text = "10 [+] true";

        //    const string report = "Binary operator <+> is not defined for types <int> and <bool>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_CompoundExpression_Reports_Undefined()
        //{
        //    string? text = @"var x = 10 
        //                 x [+=] false";

        //    string? diagnostics = @"
        //        Binary operator <+=> is not defined for types <int> and <bool>.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}



        [TestMethod]
        public void Evaluator_IfStatement_Reports_CannotConvert()
        {
            const string text = @"
                        func void main()
                        {
                            int x = 0;
                            if ([10])
                                x = 10;
                        }
                        ";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotConvertType;
            string diagnostics = string.Format(template, "int", "bool");
            AssertDiagnostics(text, diagnostics);
        }

        //[TestMethod]
        //public void Evaluator_WhileStatement_Reports_CannotConvert()
        //{
        //    const string text = @"
        //                {
        //                    var x = 0
        //                    while [10]
        //                        x = 10

        //                }
        //                ";

        //    const string report = "Cannot convert type of <int> to <bool>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_DoWhileStatement_Reports_CannotConvert()
        //{
        //    const string text = @"
        //                {
        //                    var x = 0
        //                    do
        //                        x = 10
        //                    while [10]
        //                }
        //                ";

        //    const string report = "Cannot convert type of <int> to <bool>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_CallExpression_Reports_Undefined()
        //{
        //    const string text = "[foo](42)";

        //    const string diagnostics = @"
        //        Function <foo> does not exist.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        //[TestMethod]
        //public void Evaluator_CallExpression_Reports_NotAFunction()
        //{
        //    const string text = @"
        //        {
        //            const foo = 42
        //            [foo](42)
        //        }
        //    ";

        //    const string diagnostics = @"
        //        <foo> is not a function.
        //    ";

        //    AssertDiagnostics(text, diagnostics);
        //}

        [TestMethod]
        public void VariablesCanShadowFunctions()
        {
            const string text = @"
                native void print();
                func void main()
                {
                    const int print = 42;
                    [print](""test"");
                }
            ";

            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorIdentifierIsNotAFunction;

            string diagnostics = string.Format(template, "print");

            AssertDiagnostics(text, diagnostics);
        }

        //[TestMethod]
        //public void Evaluator_ForStatement_Reports_CannotConvert_LowerBound()
        //{
        //    const string text = @"
        //                {
        //                    var result = 0
        //                    for i = [false] to 10
        //                        result = result + i

        //                }
        //                ";

        //    const string report = "Cannot convert type of <bool> to <int>.";

        //    AssertDiagnostics(text, report);
        //}

        //[TestMethod]
        //public void Evaluator_ForStatement_Reports_CannotConvert_UpperBound()
        //{
        //    const string text = @"
        //                {
        //                    var result = 0
        //                    for i = 1 to [false]
        //                        result = result + i

        //                }
        //                ";

        //    const string report = "Cannot convert type of <bool> to <int>.";

        //    AssertDiagnostics(text, report);
        //}

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

            finalReport.Warnings.RemoveAll(m => m.Code == ReportMessageCodes.W0000);

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
