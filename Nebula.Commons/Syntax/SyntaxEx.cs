using System;
using System.Collections.Generic;

namespace Nebula.Commons.Syntax
{
    public static class SyntaxEx
    {
        public static bool IsKeyword(this NodeType type) => type.ToString().EndsWith("keyword", StringComparison.OrdinalIgnoreCase);
        public static bool IsToken(this NodeType type) => !type.IsTrivia() && (type.IsKeyword() || type.ToString().EndsWith("token", StringComparison.CurrentCultureIgnoreCase));
        public static bool IsTrivia(this NodeType type) => type switch
        {
            NodeType.MultiLineCommentTrivia or NodeType.SingleLineCommentTrivia or
            NodeType.WhiteSpaceTrivia or NodeType.LinebreakTrivia or
            NodeType.SkippedTextTrivia => true,
            _ => false,
        };

        public static string? GetText(NodeType kind) => kind switch
        {
            NodeType.PlusToken => "+",
            NodeType.PlusEqualsToken => "+=",
            NodeType.SemicolonToken => ";",
            //NodeType.ColonToken => ":",
            NodeType.DoubleColonToken => "::",
            NodeType.MinusToken => "-",
            NodeType.MinusEqualsToken => "-=",
            NodeType.StarToken => "*",
            NodeType.StarEqualsToken => "*=",
            NodeType.SlashToken => "/",
            NodeType.ModuloToken => "%",
            NodeType.DotToken => ".",
            NodeType.SlashEqualsToken => "/=",
            NodeType.BangToken => "!",
            NodeType.EqualsToken => "=",
            NodeType.AmpersandToken => "&",
            NodeType.AmpersandEqualsToken => "&=",
            NodeType.DoubleAmpersandToken => "&&",
            NodeType.PipeToken => "|",
            NodeType.PipeEqualsToken => "|=",
            NodeType.DoublePipeToken => "||",
            NodeType.HatToken => "^",
            NodeType.HatEqualsToken => "^=",
            NodeType.TildeToken => "~",
            NodeType.DoubleEqualsToken => "==",
            NodeType.BangEqualsToken => "!=",
            NodeType.LessToken => "<",
            NodeType.LessOrEqualsToken => "<=",
            NodeType.GreaterToken => ">",
            NodeType.GreaterOrEqualsToken => ">=",
            NodeType.OpenParenthesisToken => "(",
            NodeType.ClosedParenthesisToken => ")",
            NodeType.OpenBracketToken => "{",
            NodeType.ClosedBracketToken => "}",
            NodeType.CommaToken => ",",
            NodeType.FalseKeyword => "false",
            NodeType.TrueKeyword => "true",
            NodeType.ConstKeyword => "const",
            NodeType.BreakKeyword => "break",
            NodeType.ContinueKeyword => "continue",
            NodeType.IfKeyword => "if",
            NodeType.ElseKeyword => "else",
            NodeType.WhileKeyword => "while",
            NodeType.DoKeyword => "do",
            NodeType.ForKeyword => "for",
            NodeType.FuncKeyword => "func",
            NodeType.ReturnKeyword => "return",
            NodeType.WaitKeyword => "wait",
            NodeType.WaitNotificationKeyword => "waittill",
            NodeType.EndOnNotificationKeyword => "endon",
            NodeType.NotifyKeyword => "notify",
            NodeType.NamespaceKeyword => "namespace",
            NodeType.NativeKeyword => "native",
            NodeType.BundleKeyword => "bundle",
            NodeType.ImportKeyword => "import",
            NodeType.OpenSquareBracketToken => "[",
            NodeType.ClosedSquareBracketToken => "]",
            _ => null,
        };

        public static int GetUnaryPrecedence(this NodeType type)
        {
            return type switch
            {
                NodeType.PlusToken or
                NodeType.MinusToken or
                NodeType.BangToken or
                NodeType.TildeToken or
                NodeType.HatToken => 6,
                _ => 0,
            };
        }

        public static int GetBinaryPrecedence(this NodeType type)
        {
            return type switch
            {
                NodeType.DotToken => 6,
                NodeType.StarToken or NodeType.SlashToken or NodeType.ModuloToken => 5,
                NodeType.PlusToken or NodeType.MinusToken => 4,
                NodeType.BangEqualsToken or NodeType.DoubleEqualsToken or NodeType.LessToken or NodeType.LessOrEqualsToken or NodeType.GreaterToken or NodeType.GreaterOrEqualsToken => 3,
                NodeType.AmpersandToken or NodeType.DoubleAmpersandToken => 2,
                NodeType.HatToken or NodeType.PipeToken or NodeType.DoublePipeToken => 1,
                _ => 0,
            };
        }

        public static NodeType GetKeywordType(string text) => text switch
        {
            "func" => NodeType.FuncKeyword,
            "bundle" => NodeType.BundleKeyword,
            "return" => NodeType.ReturnKeyword,
            "namespace" => NodeType.NamespaceKeyword,
            "async" => NodeType.AsyncKeword,
            "wait" => NodeType.WaitKeyword,
            "waittill" => NodeType.WaitNotificationKeyword,
            "notify" => NodeType.NotifyKeyword,
            "endon" => NodeType.EndOnNotificationKeyword,
            "const" => NodeType.ConstKeyword,
            "native" => NodeType.NativeKeyword,
            "break" => NodeType.BreakKeyword,
            "continue" => NodeType.ContinueKeyword,
            "if" => NodeType.IfKeyword,
            "else" => NodeType.ElseKeyword,
            "true" => NodeType.TrueKeyword,
            "false" => NodeType.FalseKeyword,
            "while" => NodeType.WhileKeyword,
            "do" => NodeType.DoKeyword,
            "for" => NodeType.ForKeyword,
            "import" => NodeType.ImportKeyword,
            _ => NodeType.IdentifierToken,
        };

        public static NodeType GetBinaryOperatorOfAssignmentOperator(NodeType kind) => kind switch
        {
            NodeType.PlusEqualsToken => NodeType.PlusToken,
            NodeType.MinusEqualsToken => NodeType.MinusToken,
            NodeType.StarEqualsToken => NodeType.StarToken,
            NodeType.SlashEqualsToken => NodeType.SlashToken,
            NodeType.AmpersandEqualsToken => NodeType.AmpersandToken,
            NodeType.PipeEqualsToken => NodeType.PipeToken,
            NodeType.HatEqualsToken => NodeType.HatToken,
            _ => throw new Exception($"Invalid syntax kind: '{kind}'"),
        };

        public static IEnumerable<NodeType> GetBinaryOperatorTypes()
        {
            NodeType[] types = (NodeType[])Enum.GetValues(typeof(NodeType));
            foreach (NodeType Type in types)
            {
                if (GetBinaryPrecedence(Type) > 0)
                {
                    yield return Type;
                }
            }
        }

        public static IEnumerable<NodeType> GetUnaryOperatorTypes()
        {
            NodeType[] types = (NodeType[])Enum.GetValues(typeof(NodeType));
            foreach (NodeType type in types)
            {
                if (GetUnaryPrecedence(type) > 0)
                {
                    yield return type;
                }
            }
        }
    }
}
