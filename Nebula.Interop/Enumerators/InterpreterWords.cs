using System;

namespace Nebula.Interop.Enumerators
{
    public static class InterpreterWords
    {
        public static string GetTokenChar(TokenType type)
        {
            switch (type)
            {
                case TokenType.OpenBody:
                    return "{";
                case TokenType.CloseBody:
                    return "}";
                case TokenType.AttributePrefix:
                    return ";";
                case TokenType.OpenParenthesis:
                    return "(";
                case TokenType.ClosedParenthesis:
                    return ")";
                case TokenType.MarkerPrefix:
                    return ".";
                case TokenType.CompiledComment:
                    return "#";
            }

            throw new Exception("Unknown marker");
        }

        public static string GetScriptSectionName(ScriptSection section, bool withPrefix)
        {
            string sectionString = section.ToString().ToLower();
            switch (section)
            {
                case ScriptSection.Function:
                    sectionString = "func";
                    break;
            }

            if (withPrefix)
            {
                sectionString = GetTokenChar(TokenType.MarkerPrefix) + sectionString;
            }

            return sectionString;
        }
    }
}
