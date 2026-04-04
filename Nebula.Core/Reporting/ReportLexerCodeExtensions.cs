using Nebula.Commons.Reporting;
using Nebula.Commons.Reporting.Strings;
using Nebula.Commons.Syntax;
using Nebula.Shared.Enumerators;

namespace Nebula.Core.Reporting
{
    public static class ReportLexerCodeExtensions
    {
        public static void ReportUnexpectedToken(this Report r, Token tokenFound, NodeType typeExpected)
        {
            (ELexerMessages code, string template) = LexerMessagesProvider.UnexpectedToken;
            string message = string.Format(template, tokenFound.Type, typeExpected);
            r.PushError(message, tokenFound.Location, code.ToString());
        }
    }
}
