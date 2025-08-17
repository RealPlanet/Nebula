using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Lexing;
using Nebula.Core.Compilation.CST.Tree;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration;
using Nebula.Core.Compilation.CST.Tree.Declaration.Bundle;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Core.Compilation.CST.Tree.Statements;
using Nebula.Core.Compilation.CST.Tree.Types;
using Nebula.Core.Reporting;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Nebula.Core.Compilation.CST.Parsing
{
    public sealed class Parser
    {
        public static CompilationUnit Parse(SourceCode source, out Report parsingReport)
        {
            Parser parser = new(source);
            CompilationUnit unit = parser.Parse();
            parsingReport = parser._parseReport;
            return unit;
        }

        private Parser(SourceCode code)
        {
            _currentSource = code;
            _currentUnit = new(_currentSource);
            _currentTokens = Lexer.ParseFrom(_currentSource, out Report? lexReport);
            _parseReport.Append(lexReport);
        }

        private CompilationUnit Parse()
        {
            if (!_parseReport.HasErrors)
            {
                ParseGlobalStatements();
            }

            if (!_parseReport.HasErrors)
            {
                Token eofToken = MatchToken(NodeType.EndOfFileToken);
                Debug.Assert(eofToken != null);

                if (_currentUnit.NamespaceStatement.IsEmpty)
                {
                    // If no namespace was set in the source code the file name is used instead
                    string defaultNamespace = Path.GetFileNameWithoutExtension(_currentSource.FileName);
                    _currentUnit.NamespaceStatement = new NamespaceStatement(defaultNamespace);
                    _parseReport.ReportNamespaceNotSet(defaultNamespace, _currentSource);
                }
            }

            return _currentUnit;
        }

        #region Parsing
        private void ParseGlobalStatements()
        {
            bool continueParsing = true;
            while (continueParsing && _currentTokenIndex < _currentTokens.Count)
            {
                switch (Current.Type)
                {
                    case NodeType.ImportKeyword:
                        {
                            continueParsing = ParseImportStatement();
                            break;
                        }
                    case NodeType.NamespaceKeyword:
                        {
                            continueParsing = ParseNamespaceName();
                            break;
                        }
                    case NodeType.FuncKeyword:
                        {
                            continueParsing = ParseFunctionDeclaration();
                            break;
                        }
                    case NodeType.NativeKeyword:
                        {
                            continueParsing = ParseNativeFunctionDeclaration();
                            break;
                        }
                    case NodeType.BundleKeyword:
                        {
                            continueParsing = ParseBundleDeclaration();
                            break;
                        }
                    case NodeType.EndOfFileToken:
                        continueParsing = false;
                        break;
                    default:
                        {
                            _parseReport.ReportUnknownGlobalStatement(Current);
                            return;
                        }
                }
            }
        }

        private bool ParseBundleDeclaration()
        {
            Token keyword = MatchToken(NodeType.BundleKeyword);
            Token bundleName = MatchToken(NodeType.IdentifierToken);

            if (_currentUnit.Bundles.Any(a => a.Name.Text == bundleName.Text))
            {
                _parseReport.ReportBundleAlreadyDefined(bundleName);
                return false;
            }

            Token openBracket = MatchToken(NodeType.OpenBracketToken);
            ImmutableArray<BundleFieldDeclaration>.Builder builder = ImmutableArray.CreateBuilder<BundleFieldDeclaration>();
            while (Current.Type != NodeType.ClosedBracketToken)
            {
                Token start = Current;
                TypeClause type = ParseTypeClause();
                Token varName = MatchToken(NodeType.IdentifierToken);
                Token semicolon = MatchToken(NodeType.SemicolonToken);
                builder.Add(new(_currentSource, type, varName, semicolon));

                // Don't get stuck in an infinite loop
                if (start == Current)
                {
                    _currentTokenIndex++;
                }
            }

            Token closebracket = MatchToken(NodeType.ClosedBracketToken);
            _currentUnit.Bundles.Add(new(_currentSource, keyword, bundleName, openBracket, builder.ToImmutableArray(), closebracket));
            return true;
        }

        private bool ParseImportStatement()
        {
            Token keyword = MatchToken(NodeType.ImportKeyword);
            Token namespaceToImport = MatchToken(NodeType.StringToken);
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            _currentUnit.Imports.Add(new(_currentSource, keyword, namespaceToImport, semicolon));
            return true;
        }

        private bool ParseNamespaceName()
        {
            Token keyword = MatchToken(NodeType.NamespaceKeyword);
            Token name = MatchToken(NodeType.StringToken);
            Token semicolon = MatchToken(NodeType.SemicolonToken);

            if (!_currentUnit.NamespaceStatement.IsEmpty)
            {
                _parseReport.ReportNamespaceAlreadySet(name);
                return false;
            }

            if (_currentUnit.Functions.Count != 0 || _currentUnit.Bundles.Count != 0)
            {
                _parseReport.ReportNamespaceMustBeFirst(name);
                return false;
            }

            NamespaceStatement statement = new(_currentSource, keyword, name, semicolon);
            _currentUnit.NamespaceStatement = statement;
            return true;
        }

        private bool ParseFunctionDeclaration()
        {
            Token keyword = MatchToken(NodeType.FuncKeyword);
            TypeClause type = ParseTypeClause();
            Token funcName = MatchToken(NodeType.IdentifierToken);
            string strFuncName = funcName.Text;
            if (_currentUnit.Functions.Any(f => f.Name.Text == strFuncName))
            {
                _parseReport.ReportFunctionAlreadyDefined(funcName);
                return false;
            }

            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);

            TokenSeparatedList<Parameter> parameters = ParseParameterList();

            Token closedParenthesis = MatchToken(NodeType.ClosedParenthesisToken);

            TokenSeparatedList<Token> attributes = ParseAttributeList();

            BlockStatement blockStatement = ParseBlockStatement();

            FunctionDeclaration newFunc = new(_currentSource, keyword, type, funcName, openParenthesis, parameters, closedParenthesis, attributes, blockStatement);
            _currentUnit.Functions.Add(newFunc);
            return true;
        }

        private bool ParseNativeFunctionDeclaration()
        {
            Token keyword = MatchToken(NodeType.NativeKeyword);
            TypeClause type = ParseTypeClause();
            Token funcName = MatchToken(NodeType.IdentifierToken);
            string strFuncName = funcName.Text;
            if (_currentUnit.Functions.Any(f => f.Name.Text == strFuncName))
            {
                _parseReport.ReportNativeFunctionAlreadyDefined(funcName);
                return false;
            }

            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);

            TokenSeparatedList<Parameter> parameters = ParseParameterList();

            Token closedParenthesis = MatchToken(NodeType.ClosedParenthesisToken);
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            _currentUnit.NativeFunction.Add(new(_currentSource, keyword, type, funcName, openParenthesis, parameters, closedParenthesis, semicolon));
            return true;
        }

        private TypeClause ParseTypeClause()
        {
            Token first = MatchToken(NodeType.IdentifierToken);

            // For bundle delcarations outside scope
            if (Current.Type == NodeType.DoubleColonToken)
            {
                Token doubleColon = MatchToken(NodeType.DoubleColonToken);
                Token typeName = MatchToken(NodeType.IdentifierToken);
                RankSpecifier? rs = ParseRankSpecifier();
                return new TypeClause(_currentSource, first, doubleColon, typeName, rs);
            }

            RankSpecifier? rankSpecifier = ParseRankSpecifier();
            return new TypeClause(_currentSource, null, null, first, rankSpecifier);
        }

        private RankSpecifier? ParseRankSpecifier()
        {
            if (Current.Type != NodeType.OpenSquareBracketToken)
            {
                return null;
            }

            Token open = MatchToken(NodeType.OpenSquareBracketToken);

            NodeType separator = NodeType.CommaToken;
            TokenSeparatedList<Token> parameters = new(separator);
            while (Current.Type != NodeType.ClosedParenthesisToken && Current.Type != NodeType.EndOfFileToken)
            {
                if (Current.Type != NodeType.CommaToken)
                {
                    break;
                }

                Token comma = MatchToken(separator);
                parameters.AppendSeparator(comma);
            }

            Token close = MatchToken(NodeType.ClosedSquareBracketToken);
            return new RankSpecifier(_currentSource, open, parameters, close);
        }

        private TokenSeparatedList<Token> ParseAttributeList()
        {
            NodeType separator = NodeType.CommaToken;
            TokenSeparatedList<Token> attributes = new(separator);

            while ((Current.Type == NodeType.CommaToken || Current.Type == NodeType.IdentifierToken) &&
                Current.Type != NodeType.EndOfFileToken)
            {
                Token identifier = MatchToken(NodeType.IdentifierToken);
                attributes.Append(identifier);

                if (Current.Type != NodeType.CommaToken)
                {
                    break;
                }

                attributes.AppendSeparator(MatchToken(NodeType.CommaToken));
            }

            return attributes;
        }

        private TokenSeparatedList<Parameter> ParseParameterList()
        {
            NodeType separator = NodeType.CommaToken;
            TokenSeparatedList<Parameter> parameters = new(separator);
            while (Current.Type != NodeType.ClosedParenthesisToken && Current.Type != NodeType.EndOfFileToken)
            {
                Parameter param = ParseParameter();
                parameters.Append(param);

                if (Current.Type != NodeType.CommaToken)
                {
                    break;
                }

                Token comma = MatchToken(separator);
                parameters.AppendSeparator(comma);
            }

            return parameters;
        }

        private Parameter ParseParameter()
        {
            TypeClause type = ParseTypeClause();
            Token identifier = MatchToken(NodeType.IdentifierToken);
            return new(_currentSource, type, identifier);
        }

        private BlockStatement ParseBlockStatement()
        {
            Token open = MatchToken(NodeType.OpenBracketToken);
            ImmutableArray<Statement>.Builder statements = ImmutableArray.CreateBuilder<Statement>();
            while (Current.Type != NodeType.ClosedBracketToken && Current.Type != NodeType.EndOfFileToken)
            {
                Token start = Current;

                Statement statement = ParseStatement();
                statements.Add(statement);

                // Don't get stuck in an infinite loop
                if (start == Current)
                {
                    _currentTokenIndex++;
                }
            }

            Token close = MatchToken(NodeType.ClosedBracketToken);
            return new(_currentSource, open, statements.ToImmutableArray(), close);
        }

        private Statement ParseStatement()
        {
            switch (Current.Type)
            {
                case NodeType.OpenBracketToken:
                    {
                        return ParseBlockStatement();
                    }
                case NodeType.ConstKeyword:
                    {
                        return ParseVariableDeclarations();
                    }
                case NodeType.WaitKeyword:
                    {
                        return ParseWaitStatement();
                    }
                case NodeType.ReturnKeyword:
                    {
                        return ParseReturnStatement();
                    }
                case NodeType.BreakKeyword:
                    {
                        return ParseBreakStatement();
                    }
                case NodeType.ContinueKeyword:
                    {
                        return ParseContinueStatement();
                    }
                case NodeType.IfKeyword:
                    {
                        return ParseIfStatement();
                    }
                case NodeType.WhileKeyword:
                    {
                        return ParseWhileStatement();
                    }
                case NodeType.ForKeyword:
                    {
                        return ParseForLoopStatement();
                    }
                default:
                    {
                        bool isBaseVariableDefinition = Current.Type == NodeType.IdentifierToken &&
                            Peek(1).Type == NodeType.IdentifierToken;

                        if (isBaseVariableDefinition)
                        {
                            return ParseVariableDeclarations();
                        }

                        bool isVariableDefinitionWithRank = Current.Type == NodeType.IdentifierToken &&
                            Peek(1).Type == NodeType.OpenSquareBracketToken &&
                           (Peek(2).Type == NodeType.CommaToken || Peek(2).Type == NodeType.ClosedSquareBracketToken);

                        if (isVariableDefinitionWithRank)
                        {
                            return ParseVariableDeclarations();
                        }

                        // This also executes if with namespace and rank
                        bool isVariableDefinitionWithNamespace = Current.Type == NodeType.IdentifierToken &&
                            Peek(1).Type == NodeType.DoubleColonToken &&
                            Peek(2).Type == NodeType.IdentifierToken &&
                            Peek(3).Type != NodeType.OpenParenthesisToken;
                        // Variable declaration with type from another namespace
                        if (isVariableDefinitionWithNamespace)
                        {
                            return ParseVariableDeclarations();
                        }

                        if (Current.Type == NodeType.IdentifierToken &&
                            Peek(1).Type == NodeType.WaitNotificationKeyword)
                        {
                            return ParseWaitNotificationStatement();
                        }

                        if (Current.Type == NodeType.IdentifierToken &&
                            Peek(1).Type == NodeType.NotifyKeyword)
                        {
                            return ParseNotifyStatement();
                        }

                        //if(Current.Type == NodeType.BundleKeyword &&
                        //    Peek(1).Type == NodeType.IdentifierToken)
                        //{
                        //    return ParseVariableDeclaration();
                        //};

                        Expression expression = ParseExpression();
                        Token token = MatchToken(NodeType.SemicolonToken);
                        ExpressionStatement exprStatement = new(_currentSource, expression, token);
                        return exprStatement;
                    }
            }
        }

        private Statement ParseNotifyStatement()
        {
            NameExpression identifier = ParseVariableNameExpression();
            Token keyword = MatchToken(NodeType.NotifyKeyword);
            Expression expression = ParseExpression();
            Token semicolon = MatchToken(NodeType.SemicolonToken);

            return new NotifyStatement(_currentSource, identifier, keyword, expression, semicolon);
        }

        private Statement ParseWaitNotificationStatement()
        {
            NameExpression identifier = ParseVariableNameExpression();
            Token keyword = MatchToken(NodeType.WaitNotificationKeyword);
            Expression expression = ParseExpression();
            Token semicolon = MatchToken(NodeType.SemicolonToken);

            return new WaitNotificationStatement(_currentSource, identifier, keyword, expression, semicolon);
        }

        private WaitStatement ParseWaitStatement()
        {
            Token keyword = MatchToken(NodeType.WaitKeyword);
            Expression time = ParseExpression();
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            return new WaitStatement(_currentSource, keyword, time, semicolon);
        }

        private ReturnStatement ParseReturnStatement()
        {
            Token keyword = MatchToken(NodeType.ReturnKeyword);
            Expression? expression = null;
            if (Current.Type != NodeType.SemicolonToken)
            {
                expression = ParseExpression();
            }

            Token semicolon = MatchToken(NodeType.SemicolonToken);
            return new ReturnStatement(_currentSource, keyword, expression, semicolon);
        }

        private ContinueStatement ParseContinueStatement()
        {
            Token keyword = MatchToken(NodeType.ContinueKeyword);
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            return new ContinueStatement(_currentSource, keyword, semicolon);
        }

        private BreakStatement ParseBreakStatement()
        {
            Token keyword = MatchToken(NodeType.BreakKeyword);
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            return new BreakStatement(_currentSource, keyword, semicolon);
        }

        private IfStatement ParseIfStatement()
        {
            Token keyword = MatchToken(NodeType.IfKeyword);
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);
            Expression condition = ParseExpression();
            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);
            Statement statement = ParseStatement();
            ElseClauseStatement? elseClause = ParseOptionalElseClause();

            return new IfStatement(_currentSource, keyword, openParenthesis, condition, closeParenthesis, statement, elseClause);
        }

        private WhileStatement ParseWhileStatement()
        {
            Token keyword = MatchToken(NodeType.WhileKeyword);
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);
            Expression condition = ParseExpression();
            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);
            Statement body = ParseStatement();
            return new WhileStatement(_currentSource, keyword, openParenthesis, condition, closeParenthesis, body);
        }

        #region For loop

        private ForStatement ParseForLoopStatement()
        {
            Token keyword = MatchToken(NodeType.ForKeyword);
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);

            // Parse init statement
            VariableDeclarationCollection initStatement = ParseForLoopInitStatement();
            Expression? condition = ParseForLoopCondition();
            Token secondSemicolon = MatchToken(NodeType.SemicolonToken);

            Expression? expression = ParseForLoopExpression();

            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);
            Statement forBody = ParseStatement();
            return new ForStatement(_currentSource, keyword, openParenthesis, initStatement, condition, secondSemicolon, expression, closeParenthesis, forBody);
        }

        /// <summary>
        /// First for loop parameter
        /// </summary>
        private VariableDeclarationCollection ParseForLoopInitStatement()
        {
            if (Current.Type == NodeType.SemicolonToken)
            {
                return new VariableDeclarationCollection(_currentSource, null, MatchToken(NodeType.SemicolonToken));
            }

            return ParseVariableDeclarations();
        }

        /// <summary>
        /// Second for loop parameter
        /// </summary>
        private Expression? ParseForLoopCondition()
        {
            if (Current.Type == NodeType.SemicolonToken)
            {
                return null;
            }

            return ParseExpression();
        }

        /// <summary>
        /// Third for loop parameter
        /// </summary>
        private Expression? ParseForLoopExpression()
        {
            if (Current.Type == NodeType.ClosedParenthesisToken)
            {
                return null;
            }

            return ParseExpression();
        }

        #endregion

        private ElseClauseStatement? ParseOptionalElseClause()
        {
            if (Current.Type is not NodeType.ElseKeyword)
            {
                return null;
            }

            Token keyword = MatchToken(NodeType.ElseKeyword);
            Statement statement = ParseStatement();

            return new(_currentSource, keyword, statement);
        }

        private VariableDeclarationCollection ParseVariableDeclarations()
        {
            bool isConst = Current.Type == NodeType.ConstKeyword;
            Token? constKeyword = null;
            if (isConst)
            {
                constKeyword = MatchToken(NodeType.ConstKeyword);
            }

            TypeClause type = ParseTypeClause();
            TokenSeparatedList<VariableDeclaration> declarations = ParseCommaSeparatedVariableDeclarations(type);
            Token semicolon = MatchToken(NodeType.SemicolonToken);
            return new VariableDeclarationCollection(_currentSource, constKeyword, declarations, semicolon);
        }

        private TokenSeparatedList<VariableDeclaration> ParseCommaSeparatedVariableDeclarations(TypeClause varType)
        {
            const NodeType separatorType = NodeType.CommaToken;
            TokenSeparatedList<VariableDeclaration> variables = new(separatorType);

            while (Current.Type != NodeType.SemicolonToken && Current.Type != NodeType.EndOfFileToken)
            {
                Token identifier = MatchToken(NodeType.IdentifierToken);
                Token equals = MatchToken(NodeType.EqualsToken);
                Expression initializer = ParseExpression();

                VariableDeclaration variable = new(_currentSource, varType, identifier, equals, initializer);
                variables.Append(variable);

                if (Current.Type != separatorType)
                {
                    break;
                }

                variables.AppendSeparator(MatchToken(separatorType));
            }

            return variables;
        }

        private Expression ParseExpression()
        {
            if (Current.Type == NodeType.IdentifierToken)
            {
                Token lookAheadOperator = PeekAfterNameExpression();
                switch (lookAheadOperator.Type)
                {
                    case NodeType.PlusEqualsToken:
                    case NodeType.MinusEqualsToken:
                    case NodeType.StarEqualsToken:
                    case NodeType.SlashEqualsToken:
                    case NodeType.AmpersandEqualsToken:
                    case NodeType.PipeEqualsToken:
                    case NodeType.HatEqualsToken:
                    case NodeType.EqualsToken:
                        {
                            Expression identifier = ParseVariableNameExpression();
                            Token op = MatchToken(Current.Type);
                            Expression rightExpr = ParseExpression();
                            return new AssignmentExpression(_currentSource, identifier, op, rightExpr);
                        }
                }
            }

            return ParseBinaryExpression();
        }

        private Expression ParseBinaryExpression(int parentPrecedence = 0)
        {
            int unaryPrecedence = Current.Type.GetUnaryPrecedence();
            Expression left;
            // Math operations have a precedence thus they must be a binary expression
            if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
            {
                // This operand has a bigger precedence of the parent, we reorder the opration to make it execute first
                Token op = MatchToken(Current.Type);
                Expression operand = ParseBinaryExpression(unaryPrecedence);
                left = new UnaryExpression(_currentSource, op, operand);
            }
            else
            {
                // This is not a math expression so parse it accordingly
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                int binaryPrecedence = Current.Type.GetBinaryPrecedence();
                if (binaryPrecedence == 0 || binaryPrecedence <= parentPrecedence)
                {
                    break;
                }

                Token op = MatchToken(Current.Type);
                Expression right = ParseBinaryExpression(binaryPrecedence);
                left = new BinaryExpression(_currentSource, left, op, right);
            }

            return left;
        }

        private Expression ParsePrimaryExpression()
        {
            switch (Current.Type)
            {
                case NodeType.OpenParenthesisToken:
                    {
                        return ParseParenthesizedExpression();
                    }
                case NodeType.TrueKeyword:
                case NodeType.FalseKeyword:
                    {
                        return ParseBoolLiteral();
                    }
                case NodeType.NumberToken:
                    {
                        return ParseNumberLiteral();
                    }
                case NodeType.StringToken:
                    {
                        return ParseStringLiteral();
                    }
                case NodeType.OpenSquareBracketToken:
                    {
                        return ParseExpressionList();
                    }
            }

            return ParseNameOrFunctionCallExpression();
        }

        private DefaultInitializationExpression ParseExpressionList()
        {
            Token open = MatchToken(NodeType.OpenSquareBracketToken);
            Token close = MatchToken(NodeType.ClosedSquareBracketToken);

            return new DefaultInitializationExpression(_currentSource, open, close);
        }

        private Expression ParseNameOrFunctionCallExpression()
        {
            Expression? functionCall = ParseFunctionCall();
            if (functionCall is not null)
            {
                return functionCall;
            }

            return ParseVariableNameExpression();
        }

        private NameExpression ParseVariableNameExpression()
        {
            Token name = MatchToken(NodeType.IdentifierToken);
            if (Current.Type == NodeType.DotToken)
            {
                Token accessToken = MatchToken(NodeType.DotToken);
                Token fieldName = MatchToken(NodeType.IdentifierToken);
                return new BundleFieldAccessExpression(_currentSource, name, accessToken, fieldName);
            }

            if (Current.Type == NodeType.OpenSquareBracketToken)
            {
                Token openSquare = MatchToken(NodeType.OpenSquareBracketToken);
                Expression accessExpression = ParseExpression();
                Token closeSquare = MatchToken(NodeType.ClosedSquareBracketToken);
                return new ArrayAccessExpression(_currentSource, name, openSquare, accessExpression, closeSquare);
            }

            return new NameExpression(_currentSource, name);
        }

        private Token PeekAfterNameExpression()
        {
            if (Current.Type != NodeType.IdentifierToken)
            {
                return Current;
            }

            if (Peek(1).Type == NodeType.DotToken)
            {
                return Peek(3);
            }

            if (Peek(1).Type == NodeType.OpenSquareBracketToken)
            {
                int peekOffset = 2;
                NodeType currentType = Peek(peekOffset).Type;
                // Skip entire expression assuming it is correct
                while (currentType != NodeType.ClosedSquareBracketToken &&
                    currentType != NodeType.EndOfFileToken)
                {
                    peekOffset++;
                    currentType = Peek(peekOffset).Type;
                }

                if (currentType == NodeType.ClosedSquareBracketToken)
                {
                    return Peek(peekOffset + 1);
                }
            }

            return Peek(1);
        }

        private Expression? ParseFunctionCall()
        {
            bool CheckIsExternal()
            {
                return Current.Type == NodeType.IdentifierToken &&
                Peek(1).Type == NodeType.DoubleColonToken &&
                Peek(2).Type == NodeType.IdentifierToken &&
                Peek(3).Type == NodeType.OpenParenthesisToken;
            }

            Token c = Current;
            bool isAsync = c.Type == NodeType.AsyncKeword;
            bool isFunctionCall = c.Type == NodeType.IdentifierToken && Peek(1).Type == NodeType.OpenParenthesisToken;
            bool isExternalFunctionCall = !isAsync && !isFunctionCall && CheckIsExternal();

            if (isAsync)
            {
                isFunctionCall = Peek(1).Type == NodeType.IdentifierToken && Peek(2).Type == NodeType.OpenParenthesisToken;
                isExternalFunctionCall = !isFunctionCall && CheckIsExternal();
            }

            if (isFunctionCall)
            {
                return ParseFunctionCall(isAsync);
            }

            if (isExternalFunctionCall)
            {
                return ParseExternalFunctionCall(isAsync);
            }

            return null;
        }

        private CallExpression ParseFunctionCall(bool isAsync)
        {
            Token? asyncCall = null;
            if (isAsync)
            {
                asyncCall = MatchToken(NodeType.AsyncKeword);
            }

            Token identifier = MatchToken(NodeType.IdentifierToken);
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);
            TokenSeparatedList<Expression> args = ParseArguments();
            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);

            return new CallExpression(_currentSource, asyncCall, null, null, identifier, openParenthesis, args, closeParenthesis);
        }

        private CallExpression ParseExternalFunctionCall(bool isAsync)
        {
            Token? asyncCall = null;
            if (isAsync)
            {
                asyncCall = MatchToken(NodeType.AsyncKeword);
            }

            Token @namespace = MatchToken(NodeType.IdentifierToken);
            Token doubleColon = MatchToken(NodeType.DoubleColonToken);
            Token identifier = MatchToken(NodeType.IdentifierToken);
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);
            TokenSeparatedList<Expression> args = ParseArguments();
            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);

            return new CallExpression(_currentSource, asyncCall, @namespace, doubleColon, identifier, openParenthesis, args, closeParenthesis);
        }

        private TokenSeparatedList<Expression> ParseArguments()
        {
            NodeType separator = NodeType.CommaToken;
            TokenSeparatedList<Expression> arguments = new(separator);
            while (Current.Type != NodeType.ClosedParenthesisToken && Current.Type != NodeType.EndOfFileToken)
            {
                Expression expr = ParseExpression();
                arguments.Append(expr);

                if (Current.Type != NodeType.CommaToken)
                {
                    break;
                }

                Token comma = MatchToken(separator);
                arguments.AppendSeparator(comma);
            }

            return arguments;
        }

        #region Literal Parsing

        private Expression ParseStringLiteral()
        {
            Token b = MatchToken(NodeType.StringToken);
            return new LiteralExpression(_currentSource, b);
        }

        private Expression ParseNumberLiteral()
        {
            Token b = MatchToken(NodeType.NumberToken);
            return new LiteralExpression(_currentSource, b);
        }

        private Expression ParseBoolLiteral()
        {
            bool isTrue = Current.Type == NodeType.TrueKeyword;
            Token b = isTrue ? MatchToken(NodeType.TrueKeyword) : MatchToken(NodeType.FalseKeyword);
            return new LiteralExpression(_currentSource, b, isTrue);
        }

        #endregion

        private Expression ParseParenthesizedExpression()
        {
            Token openParenthesis = MatchToken(NodeType.OpenParenthesisToken);
            Expression expr = ParseExpression();
            Token closeParenthesis = MatchToken(NodeType.ClosedParenthesisToken);

            return new ParenthesizedExpression(_currentSource, openParenthesis, expr, closeParenthesis);
        }

        #endregion

        private Token MatchToken(NodeType type)
        {
            if (Current.Type == type)
            {
                return _currentTokens[_currentTokenIndex++];
            }

            _parseReport.ReportUnexpectedToken(Current, type);

            return new(_currentSource, type, Current.TextPosition, null, null, ImmutableArray<Trivia>.Empty, ImmutableArray<Trivia>.Empty);
        }
        private Token Peek(int offset)
        {
            if (_currentTokenIndex + offset >= _currentTokens.Count)
            {
                return _currentTokens[_currentTokens.Count - 1];
            }

            return _currentTokens[_currentTokenIndex + offset];
        }
        private Token Current => Peek(0);

        private readonly SourceCode _currentSource;
        private readonly CompilationUnit _currentUnit;
        private readonly Report _parseReport = new();
        private readonly IReadOnlyList<Token> _currentTokens;
        private int _currentTokenIndex = 0;
    }
}
