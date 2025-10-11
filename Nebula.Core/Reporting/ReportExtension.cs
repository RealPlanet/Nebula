using Nebula.Commons.Reporting;
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
using System.Linq;

namespace Nebula.Core.Reporting
{
    /// <summary>
    /// Provides methods which build a report message and add it to the report
    /// </summary>
    public static class ReportExtension
    {
        #region Errors

        #region Token Errors
        public static void ReportUnknownGlobalStatement(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorUnexpectedGlobalStatement;
            string message = string.Format(MessageTemplate, token);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportNamespaceAlreadySet(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorNamespaceAlreadySet;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportNamespaceMustBeFirst(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorNamespaceMustBeFirstOfAny;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportBundleAlreadyDefined(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorBundleAlreadyExists;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportNativeFunctionAlreadyDefined(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorNativeFunctionAlreadyExists;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportBundleFieldAlreadyDeclared(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorFieldAlreadyDeclared;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportUndefinedType(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorTypeDoesNotExist;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportFunctionAlreadyDefined(this Report r, Token token)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorFunctionAlreadyExists;
            string message = string.Format(MessageTemplate, token.Text);
            r.PushCode(code, message, token.Location);
        }
        public static void ReportAttributeDoesNotSupportFunctionParameters(this Report r, Token identifier)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorAttributeRequiresZeroParameters;
            string message = string.Format(template, identifier.Text);
            r.PushCode(code, message, identifier.Location);
        }
        public static void ReportAttributeDoesNotSupportFunctionReturnType(this Report r, Token identifier)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorAttributeRequiresVoidReturnType;
            string message = string.Format(template, identifier.Text);
            r.PushCode(code, message, identifier.Location);
        }
        public static void ReportUnterminatedString(this Report r, TextLocation where)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorUnterminatedStringLiteral;
            string message = string.Format(MessageTemplate, where.Text);
            r.PushCode(code, message, where);
        }
        public static void ReportBundleDoesNotExist(this Report r, Token identifier)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorBundleDoesNotExist;
            string message = string.Format(template, identifier.Text);
            r.PushCode(code, message, identifier.Location);
        }
        public static void ReportFieldDoesNotExist(this Report r, Token identifier)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorFieldDoesNotExist;
            string message = string.Format(template, identifier.Text);
            r.PushCode(code, message, identifier.Location);
        }
        public static void ReportVariableAlreadyDeclared(this Report r, Token token)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorVariableAlreadyDeclared;
            string message = string.Format(template, token.Text);
            r.PushCode(code, message, token.Location);
        }

        #endregion

        #region Binder Errors
        public static void ReportBinderFunctionAlreadyExists(this Report r, BaseFunctionDeclaration func)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorBinderFunctionAlreadyExists;
            string message = string.Format(MessageTemplate, func.Name.Text);
            r.PushCode(code, message, func.Name.Location);
        }

        public static void ReportAllPathsMustReturn(this Report r, FunctionDeclaration func)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorNotAllPathsReturn;
            string message = string.Format(template, func.Name.Text);
            r.PushCode(code, message, func.Name.Location);
        }

        public static void ReportParameterAlreadyDeclared(this Report r, Parameter parameter)
        {
            (ReportMessageCodes code, string MessageTemplate) = ReportMessageProvider.ErrorParameterAlreadyDeclared;
            string message = string.Format(MessageTemplate, parameter.Identifier.Text);
            r.PushCode(code, message, parameter.Location);
        }

        public static void ReportCannotDeclareParameterVariable(this Report r, Parameter parameter)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotBindParameter;
            string message = string.Format(template, parameter.Identifier.Text);
            r.PushCode(code, message, parameter.Identifier.Location);
        }
        public static void ReportExpresionMustHaveValue(this Report r, Expression expression)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorExpressionMustHaveValue;
            r.PushCode(code, template, expression.Location);
        }
        public static void ReportInvalidExpressionStatement(this Report r, TextLocation location)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorInvalidExpressionStatement;
            r.PushCode(code, template, location);
        }
        #endregion

        #region Other
        public static void ReportCannotConvertType(this Report r, TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotConvertType;
            string message = string.Format(template, fromType, toType);
            r.PushCode(code, message, location);
        }
        public static void ReportCannotConvertTypeImplicity(this Report r, TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotConvertTypeImplicity;
            string message = string.Format(template, fromType, toType);
            r.PushCode(code, message, location);
        }
        public static void ReportWrongNumberOfArguments(this Report r, TextLocation location, string functionName, int expectedCount, int actualCount)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorWrongNumberOfArguments;
            string message = string.Format(template, functionName, expectedCount, actualCount);
            r.PushCode(code, message, location);
        }
        public static void ReportUndefinedFunction(this Report r, TextLocation location, string functionName)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorFunctionDoesNotExists;
            string message = string.Format(template, functionName);
            r.PushCode(code, message, location);
        }
        public static void ReportObjectFunctionDoesNotExist(this Report r, string objectType, TextLocation location, string functionName)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorObjectFunctionDoesNotExists;
            string message = string.Format(template, functionName, objectType);
            r.PushCode(code, message, location);
        }
        public static void ReportNotAFunction(this Report r, TextLocation location, string name)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorIdentifierIsNotAFunction;
            string message = string.Format(template, name);
            r.PushCode(code, message, location);
        }
        public static void ReportIdentifierNotOfType(this Report r, TextLocation location, string text, TypeSymbol type)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorValueNotOfType;
            string message = string.Format(template, text, type);
            r.PushCode(code, message, location);
        }
        public static void ReportFloatMustEndWithMarker(this Report r, TextLocation location, string text)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorFloatNoMarker;
            string message = string.Format(template, text);
            r.PushCode(code, message, location);
        }

        public static void TooManyDecimalPointsInNumber(this Report r, TextLocation location)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorFloatTooManyMarkers;
            r.PushCode(code, template, location);
        }

        public static void ReportUndefinedUnaryOperator(this Report r, TextLocation location, string text, TypeSymbol boundType)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorUnaryOperatorNotDefined;
            string message = string.Format(template, text, boundType);
            r.PushCode(code, message, location);
        }

        public static void ReportAssignmentLeftHandSideNotValid(this Report r, TextLocation leftHandLocation)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorAssignmentLeftHandSideNotValid;
            string message = string.Format(template);
            r.PushCode(code, message, leftHandLocation);
        }

        public static void ReportUndefinedBinaryOperator(this Report r, TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorBinaryOperatorNotDefined;
            string message = string.Format(template, operatorText, leftType, rightType);
            r.PushCode(code, message, location);
        }
        public static void ReportUndefinedVariable(this Report r, TextLocation location, string name)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorVariableDoesNotExists;
            string message = string.Format(template, name);
            r.PushCode(code, message, location);
        }
        public static void ReportPrimitiveTypesDontHaveFields(this Report r, string variable, TextLocation location)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorBundleDoesNotExist;
            string message = string.Format(template, variable.ToString());
            r.PushCode(code, message, location);
        }
        public static void ReportNotAVariable(this Report r, TextLocation location, string name)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorNameIsNotAVariable;
            string message = string.Format(template, name);
            r.PushCode(code, message, location);
        }
        public static void ReportWaitMustBeNumber(this Report r, AbstractExpression expr)
        {
            string expression = expr.OriginalNode.SourceCode.ToString(expr.OriginalNode.Span);
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorWaitMustBeNumber;
            string message = string.Format(template, expression.ToString());
            r.PushCode(code, message, expr.OriginalNode.Location);
        }
        public static void ReportCannotAssign(this Report r, TextLocation location, string name)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorCannotReassignReadonlyVariable;
            string message = string.Format(template, name);
            r.PushCode(code, message, location);
        }
        public static void ReportInvalidReturnExpression(this Report r, TextLocation location, string functionName)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorVoidFunctionCannotReturnValue;
            string message = string.Format(template, functionName);
            r.PushCode(code, message, location);
        }
        public static void ReportMissingReturnExpression(this Report r, TextLocation location, string functionName, TypeSymbol symbol)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorFunctionExpectsReturn;
            string message = string.Format(template, functionName, symbol.Name);
            r.PushCode(code, message, location);
        }
        public static void ReportUnterminatedMultiLineComment(this Report r, TextLocation location)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorUnterminatedMultilineComment;
            r.PushCode(code, template, location);
        }
        public static void ReportBadCharacter(this Report r, TextLocation location, char CurrentCharacter)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorBadCharacterInput;
            string message = string.Format(template, CurrentCharacter);
            r.PushCode(code, message, location);
        }
        public static void ReportUnexpectedToken(this Report r, Token tokenFound, NodeType typeExpected)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorUnexpectedToken;
            string message = string.Format(template, tokenFound.Type, typeExpected);
            r.PushCode(code, message, tokenFound.Location);
        }

        public static void ReportInvalidBreakOrContinue(this Report r, TextLocation location, string name)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.ErrorInvalidBreakOrContinue;
            string message = string.Format(template, name);
            r.PushCode(code, message, location);
        }

        #endregion

        #endregion

        #region Warnings

        public static void ReportNamespaceNotSet(this Report r, string namespaceToUse, SourceCode sourceCode)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.WarningNamespaceNotSet;
            string message = string.Format(template, namespaceToUse);
            r.PushCode(code, message, new(sourceCode));
        }

        public static void ReportUnreachableCode(this Report r, TextLocation location)
        {
            (ReportMessageCodes code, string template) = ReportMessageProvider.WarningUnreachableCodeDetected;
            r.PushCode(code, template, location);
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
                    r.ReportUnreachableCode(((VariableDeclaration)node).Identifier.Location);
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
