using Nebula.Commons.Reporting;
using Nebula.Commons.Reporting.Strings;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Core.Compilation.CST.Tree.Statements;
using Nebula.Shared.Enumerators;
using System.Linq;

namespace Nebula.Core.Reporting
{
    public static class ReportBinderCodeExtensions
    {
        #region Errors
        public static void ReportBinderFunctionAlreadyExists(this Report r, BaseFunctionDeclaration func)
        {
            (EBinderMessages code, string MessageTemplate) = BinderMessagesProvider.BinderFunctionAlreadyExists;
            string message = string.Format(MessageTemplate, func.Name.Text);
            r.PushError(message, func.Name.Location, code.ToString());
        }

        public static void ReportAllPathsMustReturn(this Report r, string functionName, TextLocation location)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.NotAllPathsReturn;
            string message = string.Format(template, functionName);
            r.PushError(message, location, code.ToString());
        }

        public static void ReportParameterAlreadyDeclared(this Report r, Parameter parameter)
        {
            (EBinderMessages code, string MessageTemplate) = BinderMessagesProvider.ParameterAlreadyDeclared;
            string message = string.Format(MessageTemplate, parameter.Identifier.Text);
            r.PushError(message, parameter.Location, code.ToString());
        }

        public static void ReportCannotDeclareParameterVariable(this Report r, Parameter parameter)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.CannotBindParameter;
            string message = string.Format(template, parameter.Identifier.Text);
            r.PushError(message, parameter.Identifier.Location, code.ToString());
        }
        public static void ReportExpresionMustHaveValue(this Report r, Expression expression)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.ExpressionMustHaveValue;
            r.PushError(template, expression.Location, code.ToString());
        }
        public static void ReportInvalidExpressionStatement(this Report r, TextLocation location)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.InvalidExpressionStatement;
            r.PushError(template, location, code.ToString());
        }

        public static void ReportCannotConvertType(this Report r, TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertType;
            string message = string.Format(template, fromType, toType);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportCannotConvertTypeImplicity(this Report r, TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.CannotConvertTypeImplicity;
            string message = string.Format(template, fromType, toType);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportWrongNumberOfArguments(this Report r, TextLocation location, string functionName, int expectedCount, int actualCount)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.WrongNumberOfArguments;
            string message = string.Format(template, functionName, expectedCount, actualCount);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportUndefinedFunction(this Report r, TextLocation location, string functionName)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.FunctionDoesNotExists;
            string message = string.Format(template, functionName);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportObjectFunctionDoesNotExist(this Report r, string objectType, TextLocation location, string functionName)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.ObjectFunctionDoesNotExists;
            string message = string.Format(template, functionName, objectType);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportNotAFunction(this Report r, TextLocation location, string name)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.IdentifierIsNotAFunction;
            string message = string.Format(template, name);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportIdentifierNotOfType(this Report r, TextLocation location, string text, TypeSymbol type)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.ValueNotOfType;
            string message = string.Format(template, text, type);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportFloatMustEndWithMarker(this Report r, TextLocation location, string text)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.FloatNoMarker;
            string message = string.Format(template, text);
            r.PushError(message, location, code.ToString());
        }

        public static void TooManyDecimalPointsInNumber(this Report r, TextLocation location)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.FloatTooManyMarkers;
            r.PushError(template, location, code.ToString());
        }

        public static void ReportUndefinedUnaryOperator(this Report r, TextLocation location, string text, TypeSymbol boundType)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.UnaryOperatorNotDefined;
            string message = string.Format(template, text, boundType);
            r.PushError(message, location, code.ToString());
        }

        public static void ReportAssignmentLeftHandSideNotValid(this Report r, TextLocation leftHandLocation)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.AssignmentLeftHandSideNotValid;
            string message = string.Format(template);
            r.PushError(message, leftHandLocation, code.ToString());
        }

        public static void ReportUndefinedBinaryOperator(this Report r, TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.BinaryOperatorNotDefined;
            string message = string.Format(template, operatorText, leftType, rightType);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportUndefinedVariable(this Report r, TextLocation location, string name)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.VariableDoesNotExists;
            string message = string.Format(template, name);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportPrimitiveTypesDontHaveFields(this Report r, string variable, TextLocation location)
        {
            // todo
            (EBinderMessages code, string template) = BinderMessagesProvider.AllPathsMustReturn;
            string message = string.Format(template, variable.ToString());
            r.PushError(message, location, code.ToString());
        }
        public static void ReportNotAVariable(this Report r, TextLocation location, string name)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.NameIsNotAVariable;
            string message = string.Format(template, name);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportWaitMustBeNumber(this Report r, AbstractExpression expr)
        {
            string expression = expr.OriginalNode.SourceCode.ToString(expr.OriginalNode.Span);
            (EBinderMessages code, string template) = BinderMessagesProvider.WaitMustBeNumber;
            string message = string.Format(template, expression.ToString());
            r.PushError(message, expr.OriginalNode.Location, code.ToString());
        }
        public static void ReportCannotAssign(this Report r, TextLocation location, string name)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.CannotReassignReadonlyVariable;
            string message = string.Format(template, name);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportInvalidReturnExpression(this Report r, TextLocation location, string functionName)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.VoidFunctionCannotReturnValue;
            string message = string.Format(template, functionName);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportMissingReturnExpression(this Report r, TextLocation location, string functionName, TypeSymbol symbol)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.FunctionExpectsReturn;
            string message = string.Format(template, functionName, symbol.Name);
            r.PushError(message, location, code.ToString());
        }
        public static void ReportUnterminatedMultiLineComment(this Report r, TextLocation location)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.UnterminatedMultilineComment;
            r.PushError(template, location, code.ToString());
        }
        public static void ReportBadCharacter(this Report r, TextLocation location, char CurrentCharacter)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.BadCharacterInput;
            string message = string.Format(template, CurrentCharacter);
            r.PushError(message, location, code.ToString());
        }

        public static void ReportInvalidBreakOrContinue(this Report r, TextLocation location, string name)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.InvalidBreakOrContinue;
            string message = string.Format(template, name);
            r.PushError(message, location, code.ToString());
        }

        public static void ReportInternalErrorCouldNotDeclareStaticConstructor(this Report r, string ctorName)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.CouldNotDeclareStaticCTor;
            string message = string.Format(template, ctorName);
            r.PushError(message);
        }

        #endregion

        #region Warnings

        public static void ReportNamespaceNotSet(this Report r, string namespaceToUse, SourceCode sourceCode)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.NamespaceNotSet;
            string message = string.Format(template, namespaceToUse);
            r.PushWarning(message, new(sourceCode), code.ToString());
        }

        public static void ReportUnreachableCode(this Report r, TextLocation location)
        {
            (EBinderMessages code, string template) = BinderMessagesProvider.UnreachableCodeDetected;
            r.PushWarning(template, location, code.ToString());
        }

        public static void ReportUnreachableCode(this Report r, Node node)
        {
            switch (node.Type)
            {
                case NodeType.BlockStatement:
                    Statement? firstStatement = ((BlockStatement)node).Statements.FirstOrDefault();
                    // Report just for non empty blocks.
                    if (firstStatement != null)
                    {
                        r.ReportUnreachableCode(firstStatement);
                    }

                    return;
                case NodeType.VariableDeclaration:
                    r.ReportUnreachableCode(((VariableDeclaration)node).AssignmentExpression.Identifier.Location);
                    return;
                case NodeType.IfStatement:
                    r.ReportUnreachableCode(((IfStatement)node).IfKeyword.Location);
                    return;
                case NodeType.WhileStatement:
                    r.ReportUnreachableCode(((WhileStatement)node).WhileKeyword.Location);
                    return;
                case NodeType.DoWhileStatement:
                    r.ReportUnreachableCode(((DoWhileStatement)node).DoKeyword.Location);
                    return;
                //case NodeType.FOR_STATEMENT:
                //    r.ReportUnreachableCode(((ForStatement)node).Keyword.Location);
                //    return;
                case NodeType.BreakStatement:
                    r.ReportUnreachableCode(((BreakStatement)node).Keyword.Location);
                    return;
                case NodeType.ContinueStatement:
                    r.ReportUnreachableCode(((ContinueStatement)node).Keyword.Location);
                    return;
                case NodeType.ReturnStatement:
                    r.ReportUnreachableCode(((ReturnStatement)node).ReturnKeyword.Location);
                    return;
                case NodeType.ExpressionStatement:
                    Expression? expression = ((ExpressionStatement)node).Expression;
                    r.ReportUnreachableCode(expression);
                    return;
                case NodeType.CallExpression:
                    r.ReportUnreachableCode(((CallExpression)node).Identifier.Location);
                    return;
                default:
                    r.ReportUnreachableCode(node.Location);
                    break;
            }
        }

        #endregion
    }
}
