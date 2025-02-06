using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Binding.Symbols;
using Nebula.Core.Compilation;
using Nebula.Core.Reporting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nebula.Core.Parsing.Lexing
{
    public sealed class Lexer
    {
        public static IReadOnlyList<Token> ParseFrom(SourceCode source, out Report report)
        {
            Lexer lexer = new();
            lexer.Lex(source);
            report = lexer._report;
            return lexer._tokens;
        }

        #region Private

        private Lexer() { Reset(); }

        #region Lexing

        private void Lex(SourceCode source)
        {
            Reset();
            _source = source;

            do
            {
                LexTrivia(leading: true);
                ImmutableArray<Trivia> leadingTrivia = _triviaBuilder.ToImmutableArray();

                int tokenStart = _tokenStart;
                LexToken();
                NodeType tokenType = _type;
                object? tokenValue = _tokenValue;
                int tokenLength = _currentPosition - _tokenStart;

                LexTrivia(leading: false);
                ImmutableArray<Trivia> trailingTrivia = _triviaBuilder.ToImmutableArray();

                string? tokenText = SyntaxEx.GetText(tokenType) ?? source.ToString(tokenStart, tokenLength);
                Token newToken = new(_source, tokenType, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia);
                _tokens.Add(newToken);
            } while (_tokens.Last().Type != NodeType.EndOfFileToken);
        }

        private void LexToken()
        {
            SkipWhitespace();
            _tokenStart = _currentPosition;
            _tokenValue = null;
            _type = NodeType.Error;
            switch (Current)
            {
                case '\0':
                    {
                        _type = NodeType.EndOfFileToken;
                        return;
                    }
                case '{':
                    {
                        _type = NodeType.OpenBracketToken;
                        _currentPosition++;
                        break;
                    }
                case '}':
                    {
                        _type = NodeType.ClosedBracketToken;
                        _currentPosition++;
                        break;
                    }
                case '(':
                    {
                        _type = NodeType.OpenParenthesisToken;
                        _currentPosition++;
                        break;
                    }
                case ')':
                    {
                        _type = NodeType.ClosedParenthesisToken;
                        _currentPosition++;
                        break;
                    }
                case ';':
                    {
                        _type = NodeType.SemicolonToken;
                        _currentPosition++;
                        break;
                    }
                case '\"':
                    {
                        LexString();
                        break;
                    }
                case '.':
                    {
                        if (char.IsDigit(Peek(1)))
                        {
                            LexNumber();
                            break;
                        }

                        _type = NodeType.DotToken;
                        _currentPosition++;
                        break;
                    }
                case ',':
                    {
                        _type = NodeType.CommaToken;
                        _currentPosition++;

                        break;
                    }
                case '=':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.DoubleEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.EqualsToken;
                        _currentPosition++;

                        break;
                    }
                case '*':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.StarEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.StarToken;
                        _currentPosition++;

                        break;
                    }
                case '/':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.SlashEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.SlashToken;
                        _currentPosition++;

                        break;
                    }
                case '+':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.PlusEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.PlusToken;
                        _currentPosition++;

                        break;
                    }
                case '-':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.MinusEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.MinusToken;
                        _currentPosition++;

                        break;
                    }
                case '%':
                    {
                        _type = NodeType.ModuloToken;
                        _currentPosition++;
                        break;
                    }
                case ':':
                    {
                        if (Peek(1) == ':')
                        {
                            _type = NodeType.DoubleColonToken;
                            _currentPosition += 2;
                            break;
                        }
                        break;
                    }
                case '&':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.AmpersandEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        if (Peek(1) == '&')
                        {
                            _type = NodeType.DoubleAmpersandToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.AmpersandToken;
                        _currentPosition++;

                        break;
                    }
                case '|':
                    {

                        if (Peek(1) == '=')
                        {
                            _type = NodeType.PipeEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        if (Peek(1) == '|')
                        {
                            _type = NodeType.DoublePipeToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.PipeToken;
                        _currentPosition++;

                        break;
                    }
                case '~':
                    {
                        //if (Peek(1) == '=')
                        //{
                        //    _type = NodeType.TildeToken;
                        //    _currentPosition += 2;
                        //
                        //    break;
                        //}

                        _type = NodeType.TildeToken;
                        _currentPosition++;

                        break;
                    }
                case '^':
                    {

                        if (Peek(1) == '=')
                        {
                            _type = NodeType.HatEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.HatToken;
                        _currentPosition++;

                        break;
                    }
                case '!':
                    {

                        if (Peek(1) == '=')
                        {
                            _type = NodeType.BangEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.BangToken;
                        _currentPosition++;

                        break;
                    }
                case '<':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.LessOrEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.LessToken;
                        _currentPosition++;

                        break;
                    }
                case '>':
                    {
                        if (Peek(1) == '=')
                        {
                            _type = NodeType.GreaterOrEqualsToken;
                            _currentPosition += 2;

                            break;
                        }

                        _type = NodeType.GreaterToken;
                        _currentPosition++;

                        break;
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        LexNumber();
                        break;
                    }
                case '_':
                    {
                        LexLiteral();
                        break;
                    }
                default:
                    {
                        if (!char.IsLetter(Current))
                        {
                            TextSpan span = new(_currentPosition, 1);
                            TextLocation location = new(_source, span);
                            _report.ReportBadCharacter(location, Current);
                            _currentPosition++;
                            break;
                        }

                        LexLiteral();
                        break;
                    }
            }

            Debug.Assert(_tokenStart != _currentPosition);
            if (_tokenStart == _currentPosition)
                _currentPosition++;
        }

        #region Trivia

        private void LexTrivia(bool leading)
        {
            _triviaBuilder.Clear();
            bool isDone = false;
            while (!isDone)
            {
                _tokenStart = _currentPosition;
                _type = NodeType.Error;
                _tokenValue = null;

                switch (Current)
                {
                    case '\0':
                        isDone = true;
                        break;
                    case '/':
                        {
                            if (Peek(1) == '/')
                            {
                                LexSingleLineComment();
                                break;
                            }

                            if (Peek(1) == '*')
                            {
                                LexMultiLineComment();
                                break;
                            }
                            isDone = true;
                            break;
                        }
                    case '\r':
                    case '\n':
                        {
                            // if the trivia is trailing then it stops at the first line break or whitespace
                            if (!leading)
                                isDone = true;

                            ReadLineBreak();
                            break;
                        }
                    default:
                        // This function is a more expensive check for whitespace if the four most common cases don't occour.
                        if (char.IsWhiteSpace(Current))
                        {
                            ReadWhiteSpace();
                            break;
                        }

                        isDone = true;
                        break;
                }
                int triviaLength = _currentPosition - _tokenStart;
                if (triviaLength > 0)
                {
                    string text = _source.ToString(_tokenStart, triviaLength);
                    Trivia trivia = new(_source, _type, _tokenStart, text);
                    _triviaBuilder.Add(trivia);
                }
            }
        }

        private void ReadWhiteSpace()
        {
            bool done = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        done = true;
                        break;
                    default:
                        if (!char.IsWhiteSpace(Current))
                            done = true;
                        else
                            _currentPosition++;
                        break;
                }
            }

            _type = NodeType.WhiteSpaceTrivia;
        }

        private void ReadLineBreak()
        {
            if (Current == '\r' && Peek(1) == '\n')
            {
                _currentPosition += 2;
            }
            else
            {
                _currentPosition++;
            }

            _type = NodeType.LinebreakTrivia;
        }

        private void LexSingleLineComment()
        {
            // Skip leading slash
            _currentPosition += 2;
            bool done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\r':
                    case '\n':
                    case '\0':
                        done = true;
                        break;
                    default:
                        _currentPosition++;
                        break;
                }
            }

            _type = NodeType.SingleLineCommentTrivia;
        }

        private void LexMultiLineComment()
        {
            // Skip leading slash
            _currentPosition += 2;
            bool done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        TextSpan span = new(_tokenStart, 2);
                        TextLocation location = new(_source, span);
                        _report.ReportUnterminatedMultiLineComment(location);
                        done = true;
                        break;
                    case '*':
                        if (Peek(1) == '/')
                        {
                            done = true;
                            _currentPosition++;
                        }
                        _currentPosition++;
                        break;
                    default:
                        _currentPosition++;
                        break;
                }
            }

            _type = NodeType.MultiLineCommentTrivia;
        }

        #endregion

        private void LexNumber()
        {
            _type = NodeType.NumberToken;
            bool hasFoundDecimal = false;
            while ((char.IsDigit(Current) || (Current == '.' && !hasFoundDecimal)) &&
                !char.IsWhiteSpace(Current) &&
                Current != 0)
            {
                if (Current == '.')
                    hasFoundDecimal = true;

                _currentPosition++;
            }

            int len = _currentPosition - _tokenStart;
            string numberText = _source.ToString(_tokenStart, len);
            TextSpan span = new(_tokenStart, len);
            TextLocation location = new(_source, span);

            if (hasFoundDecimal)
            {
                if (Current != 'f')
                {
                    _report.ReportFloatMustEndWithMarker(location, numberText);
                    return;
                }
                _currentPosition++;
                if (!float.TryParse(numberText, out float val))
                {
                    _report.ReportIdentifierNotOfType(location, numberText, TypeSymbol.Int);
                }

                _tokenValue = val;
            }
            else
            {
                if (!int.TryParse(numberText, out int val))
                {
                    _report.ReportIdentifierNotOfType(location, numberText, TypeSymbol.Int);
                }

                _tokenValue = val;
            }
        }
        private void LexString()
        {
            _currentPosition++; // "

            StringBuilder builder = new();
            bool endLex = false;
            while (!endLex)
            {
                switch (Current)
                {
                    case '\"':
                        {
                            if (Peek(1) == '\"')
                            {
                                builder.Append('"');
                                _currentPosition += 2;
                                break;
                            }

                            _currentPosition++;
                            endLex = true;
                            break;
                        }
                    case '\0':
                    case '\r':
                    case '\n':
                        {
                            TextLocation where = new(_source, new(_tokenStart, 1));
                            _report.ReportUnterminatedString(where);
                            endLex = true;
                            break;
                        }
                    default:
                        builder.Append(Current);
                        _currentPosition++;
                        break;
                }
            }

            _type = NodeType.StringToken;
            _tokenValue = builder.ToString();
        }
        private void LexLiteral()
        {
            while ((char.IsLetterOrDigit(Current) || Current == '_') &&
                !char.IsWhiteSpace(Current) &&
                Current != 0)
            {
                _currentPosition++;
            }

            string text = _source.ToString(_tokenStart, _currentPosition - _tokenStart);
            _type = SyntaxEx.GetKeywordType(text);
        }
        #endregion

        private char Peek(int offset)
        {
            if (_currentPosition + offset >= _source.Text.Length)
            {
                return '\0';
            }

            return _source.Text[_currentPosition + offset];
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Current))
            {
                _currentPosition++;
            }
        }

        private void Reset()
        {
            _source = null!;
            _tokens.Clear();
            _report.Clear();
            _currentPosition = 0;
            _type = NodeType.Error;
        }

        private char Current => Peek(0);

        private readonly List<Token> _tokens = new();
        private readonly Report _report = new();
        private SourceCode _source = null!;

        private int _currentPosition, _tokenStart;
        private object? _tokenValue;
        private NodeType _type;

        private readonly ImmutableArray<Trivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<Trivia>();

        #endregion
    }
}