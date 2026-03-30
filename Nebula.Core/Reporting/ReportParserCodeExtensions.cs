using Nebula.Commons.Reporting;
using Nebula.Commons.Reporting.Strings;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Shared.Enumerators;

namespace Nebula.Core.Reporting
{
    public static class ReportParserCodeExtensions
    {
        public static void ReportUnknownGlobalStatement(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.UnexpectedGlobalStatement;
            string message = string.Format(template, token);
            r.PushError(message, token.Location);
        }
        public static void ReportNamespaceAlreadySet(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.NamespaceAlreadySet;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportNamespaceMustBeFirst(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.NamespaceMustBeFirstOfAny;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportBundleAlreadyDefined(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.BundleAlreadyExists;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportNativeFunctionAlreadyDefined(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.NativeFunctionAlreadyExists;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportBundleFieldAlreadyDeclared(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.FieldAlreadyDeclared;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportUndefinedType(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.TypeDoesNotExist;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportFunctionAlreadyDefined(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.FunctionAlreadyExists;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
        public static void ReportAttributeDoesNotSupportFunctionParameters(this Report r, Token identifier)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.AttributeRequiresZeroParameters;
            string message = string.Format(template, identifier.Text);
            r.PushError(message, identifier.Location);
        }
        public static void ReportAttributeDoesNotSupportFunctionReturnType(this Report r, Token identifier)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.AttributeRequiresVoidReturnType;
            string message = string.Format(template, identifier.Text);
            r.PushError(message, identifier.Location);
        }
        public static void ReportUnterminatedString(this Report r, TextLocation where)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.UnterminatedStringLiteral;
            string message = string.Format(template, where.Text);
            r.PushError(message, where);
        }

        public static void ReportBundleDoesNotExist(this Report r, Token identifier)
        {
            r.ReportBundleDoesNotExist(identifier.Text, identifier.Location);
        }

        public static void ReportBundleDoesNotExist(this Report r, string name, TextLocation location)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.BundleDoesNotExist;
            string message = string.Format(template, name);
            r.PushError(message, location);
        }

        public static void ReportFieldDoesNotExist(this Report r, Token identifier)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.FieldDoesNotExist;
            string message = string.Format(template, identifier.Text);
            r.PushError(message, identifier.Location);
        }

        public static void ReportVariableAlreadyDeclared(this Report r, Token token)
        {
            (EParserMessages code, string template) = ParserMessagesProvider.VariableAlreadyDeclared;
            string message = string.Format(template, token.Text);
            r.PushError(message, token.Location);
        }
    }
}
