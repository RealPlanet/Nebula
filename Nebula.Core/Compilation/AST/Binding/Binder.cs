using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Binding.Symbols;
using Nebula.Core.Compilation;
using Nebula.Core.Compilation.AST;
using Nebula.Core.Graph;
using Nebula.Core.Lowering;
using Nebula.Core.Parsing;
using Nebula.Core.Parsing.Expressions;
using Nebula.Core.Parsing.Statements;
using Nebula.Core.Reporting;
using Nebula.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nebula.Core.Binding
{
    public sealed class Binder
    {
        private Binder(ICollection<CompilationUnit> units, ICollection<CompiledScript> references)
        {
            _allUnitsToBind = units.ToList();
            _allScriptToReference = references.ToList();
        }

        private Binder(CompilationUnit unit, ICollection<CompiledScript> references)
            : this([unit], references)
        {
        }

        private readonly List<CompilationUnit> _allUnitsToBind;
        private readonly List<CompiledScript> _allScriptToReference;
        private readonly Dictionary<CompilationUnit, Scope> _allParentScopes = new();

        private readonly Report _binderReport = new();

        // The current scope for symbols
        private Scope _currentScope = null!;
        // The current unit being bound
        private CompilationUnit _currentUnit = null!;
        private AbstractProgram _currentProgram = null!;
        private FunctionSymbol? _currentFunction = null;

        private readonly Stack<(AbstractLabel BreakLabel, AbstractLabel ContinueLabel)> _loopStack = new();
        private int _labelCounter = 0;

        public static ICollection<AbstractProgram> Bind(ICollection<CompilationUnit> units, ICollection<CompiledScript> references, out Report bindingReport)
        {
            Binder binder = new(units, references);
            ICollection<AbstractProgram> abstractPrograms = binder.Bind(out bindingReport);
            return abstractPrograms;
        }

        private ICollection<AbstractProgram> Bind(out Report bindingReport)
        {
            Lowerer lowerer = new();

            // Prepare all compilation units
            // Bind namespaces and bundle definitions
            Dictionary<CompilationUnit, AbstractProgram> allPrograms = new();
            foreach (CompilationUnit unit in _allUnitsToBind)
            {
                _currentUnit = unit;
                _currentScope = new(null);

                AbstractNamespace boundNamespace = BindNamespaceStatement(_currentUnit.NamespaceStatement);
                AbstractProgram newProgram = new(unit.Source, boundNamespace);
                _currentProgram = newProgram;
                _allParentScopes.Add(unit, _currentScope);

                allPrograms.Add(unit, newProgram);

                Dictionary<string, BundleSymbol> bundles = new();
                Dictionary<FunctionSymbol, AbstractBlockStatement> functions = new();

                foreach (BundleDeclaration bundle in _currentUnit.Bundles)
                {
                    BundleSymbol boundBundle = BindBundleDeclaration(bundle);
                    _currentProgram.Bundles.Add(boundBundle.Name, boundBundle);
                }
            }

            // Now we have all necessary types and we can bind the function declarations
            foreach (CompilationUnit unit in _allUnitsToBind)
            {
                _currentUnit = unit;
                _currentScope = _allParentScopes[_currentUnit];
                _currentProgram = allPrograms[_currentUnit];

                foreach (ImportStatement import in _currentUnit.Imports)
                {
                    // Use a dictionary with namespaces for faster lookup?
                    AbstractProgram? otherPorgram = allPrograms.FirstOrDefault(p => p.Value.Namespace.Text == import.Namespace).Value;
                    if (otherPorgram != null)
                    {
                        _currentProgram.References.AddAbstractProgramReference(otherPorgram);
                        continue;
                    }

                    CompiledScript? scriptReference = _allScriptToReference.FirstOrDefault(s => s.Namespace == import.Namespace);
                    if (scriptReference != null)
                    {
                        _currentProgram.References.AddScriptReference(scriptReference);
                        continue;
                    }

                    _binderReport.PushError($"Import '{import.Namespace}' not found!");
                }

                // Now that all types have been declared we can bind the function declarations
                foreach (NativeFunctionDeclaration function in _currentUnit.NativeFunction)
                {
                    FunctionSymbol nativeFunction = BindNativeFunctionDeclaration(function);
                    _currentProgram.NativeFunctions.Add(nativeFunction);
                }

                foreach (FunctionDeclaration function in _currentUnit.Functions)
                {
                    FunctionSymbol boundFunction = BindFunctionDeclaration(function);
                    _currentProgram.Functions.Add(boundFunction, null!);
                }
            }

            // Now that we bound the declarations we can bind the code for each unit and link up any reference
            foreach (CompilationUnit unit in _allUnitsToBind)
            {
                _currentUnit = unit;
                _currentScope = _allParentScopes[_currentUnit];
                _currentProgram = allPrograms[_currentUnit];

                foreach (FunctionSymbol declaration in _currentProgram.Functions.Keys)
                {
                    _currentFunction = declaration;
                    AbstractStatement body = BindStatement(declaration.Declaration!.Body);
                    AbstractBlockStatement loweredBody = lowerer.Lower(declaration, body);
                    if (declaration.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    {
                        _binderReport.ReportAllPathsMustReturn(declaration.Declaration);
                    }

                    Debug.Assert(loweredBody != null);
                    _currentProgram.Functions[declaration] = loweredBody;
                }
            }

            _currentProgram = null!;
            _currentUnit = null!;
            _currentFunction = null!;
            _currentScope = null!;

            bindingReport = _binderReport;
            return allPrograms.Values;
        }

        #region Conversion Binding
        private AbstractExpression BindConversion(Expression syntax, TypeSymbol type, bool allowExplicit = false)
        {
            AbstractExpression expression = BindExpression(syntax);
            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private AbstractExpression BindExpression(Expression syntax, TypeSymbol targetType) => BindConversion(syntax, targetType);

        private AbstractExpression BindConversion(TextLocation reportLocation, AbstractExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            TypeConversion conversion = TypeConversion.Classify(expression.ResultType, type);

            if (!conversion.Exists)
            {
                if (expression.ResultType != TypeSymbol.Error && type != TypeSymbol.Error)
                    _binderReport.ReportCannotConvertType(reportLocation, expression.ResultType, type);

                return new AbstractErrorExpression(expression.OriginalNode);
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _binderReport.ReportCannotConvertTypeImplicity(reportLocation, expression.ResultType, type);
                return new AbstractErrorExpression(expression.OriginalNode);
            }

            if (conversion.IsIdentity)
                return expression;

            return new AbstractConversionExpression(expression.OriginalNode, type, expression);
        }

        #endregion

        #region Variable Binding

        /// <summary>
        /// Bind an CST Variable declaration
        /// </summary>
        private AbstractVariableDeclarationCollection BindVariableDeclarations(VariableDeclarationCollection node)
        {
            bool isReadOnly = node.IsConst;

            ImmutableArray<AbstractVariableDeclaration>.Builder boundVariables = ImmutableArray.CreateBuilder<AbstractVariableDeclaration>();
            foreach (VariableDeclaration declaration in node.Declarations)
            {
                AbstractVariableDeclaration abstractDeclaration = BindVariableDeclaration(isReadOnly, declaration);
                boundVariables.Add(abstractDeclaration);
            }

            return new AbstractVariableDeclarationCollection(node, boundVariables.ToImmutableArray());
        }

        private AbstractVariableDeclaration BindVariableDeclaration(bool isReadOnly, VariableDeclaration declaration)
        {
            TypeSymbol type = BindTypeClause(declaration.VarType);
            AbstractExpression initializer = BindExpression(declaration.Initializer);
            VariableSymbol variable = BindVariableDeclaration(declaration.Identifier, isReadOnly, type, initializer.ConstantValue);
            AbstractExpression? convertedInitializer = BindConversion(declaration.Location, initializer, type);
            return new(declaration, variable, convertedInitializer);
        }

        /// <summary>
        /// Try to declare this variable within the scope
        /// </summary>
        private VariableSymbol BindVariableDeclaration(Token identifier, bool isReadOnly, TypeSymbol type, AbstractConstant? constant = null)
        {
            string? name = identifier.Text ?? "?";
            //VariableSymbol variable = Function == null
            //                            ? new GlobalVariableSymbol(name, isReadOnly, type, constant)
            //                            : new LocalVariableSymbol(name, isReadOnly, type, constant);


            VariableSymbol variable = new LocalVariableSymbol(name, isReadOnly, type, constant);
            // Should never happen as shadowing is allowed and we created a new scope
            if (!identifier.IsMissing && !_currentScope.TryDeclareVariable(variable))
                _binderReport.ReportVariableAlreadyDeclared(identifier);

            return variable;
        }

        private VariableSymbol? BindVariableReference(Token identifierToken)
        {
            string name = identifierToken.Text;
            switch (_currentScope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    _binderReport.ReportUndefinedVariable(identifierToken.Location, name);
                    return null;

                default:
                    _binderReport.ReportNotAVariable(identifierToken.Location, name);
                    return null;
            }
        }

        #endregion

        #region Control-Flow Loops

        private AbstractWhileStatement BindWhileStatement(WhileStatement syntax)
        {
            AbstractExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if (!(bool)condition.ConstantValue.Value)
                {
                    _binderReport.ReportUnreachableCode(syntax.Body);
                }
            }

            AbstractStatement body = BindLoopBody(syntax.Body, out AbstractLabel breakLabel, out AbstractLabel continueLabel);
            return new AbstractWhileStatement(syntax, condition, body, breakLabel, continueLabel);
        }

        private AbstractDoWhileStatement BindDoWhileStatement(DoWhileStatement syntax)
        {
            AbstractExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            if (condition.ConstantValue != null)
            {
                if (!(bool)condition.ConstantValue.Value)
                {
                    _binderReport.ReportUnreachableCode(syntax.Body);
                }
            }

            AbstractStatement body = BindLoopBody(syntax.Body, out AbstractLabel breakLabel, out AbstractLabel continueLabel);
            return new AbstractDoWhileStatement(syntax, body, condition, breakLabel, continueLabel);
        }

        private AbstractStatement BindForStatement(ForStatement syntax)
        {
            // Nuovo scope, cosi le variabili dell'init statement possono essere riutilizzate per piu for
            _currentScope = new(_currentScope);

            AbstractStatement initStatement = BindStatement(syntax.InitStatement);
            AbstractExpression? condition = null;
            if (syntax.Condition != null)
            {
                condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
                if (condition.ConstantValue != null)
                {
                    if (!(bool)condition.ConstantValue.Value)
                    {
                        _binderReport.ReportUnreachableCode(syntax.Body);
                    }
                }
            }

            AbstractExpressionStatement? statement = null;
            if (syntax.Expression != null)
            {
                AbstractExpression boundExpression = BindExpression(syntax.Expression);
                statement = new AbstractExpressionStatement(syntax.Expression, boundExpression);
            }

            AbstractStatement body = BindLoopBody(syntax.Body, out AbstractLabel breakLabel, out AbstractLabel continueLabel);
            _currentScope = _currentScope.Parent!;
            return new AbstractForStatement(syntax, initStatement, condition, statement, body, breakLabel, continueLabel);
        }

        private AbstractStatement BindLoopBody(Statement body, out AbstractLabel breakLabel, out AbstractLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new AbstractLabel($"Break{_labelCounter}");
            continueLabel = new AbstractLabel($"Continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            AbstractStatement result = BindStatement(body);
            _loopStack.Pop();
            return result;
        }

        #endregion

        #region Bundle binding
        private BundleSymbol BindBundleDeclaration(BundleDeclaration bundle)
        {
            ImmutableArray<AbstractBundleField>.Builder bundleFields = ImmutableArray.CreateBuilder<AbstractBundleField>();
            HashSet<string> seenNames = new();

            foreach (BundleFieldDeclaration field in bundle.Fields)
            {
                string name = field.Identifier.Text;
                if (!seenNames.Add(name))
                {
                    _binderReport.ReportBundleFieldAlreadyDeclared(field.Identifier);
                    continue;
                }

                TypeSymbol parameterType = BindTypeClause(field.FieldType);
                AbstractBundleField paramSymbol = new(parameterType, name, bundleFields.Count);
                bundleFields.Add(paramSymbol);
            }

            BundleSymbol boundBundle = new(bundle.Name.Text, bundle, bundleFields.ToImmutableArray());
            if (bundle.Name.Text != null && _currentProgram.Bundles.TryGetValue(bundle.Name.Text, out BundleSymbol? _))
            {
                _binderReport.ReportBundleAlreadyDefined(bundle.Name);
            }

            return boundBundle;
        }

        private AbstractExpression BindBundleFieldAssignmentExpression(AssignmentExpression syntax)
        {
            BundleFieldAccessExpression bundleAccess = (BundleFieldAccessExpression)syntax.Identifier;

            string? name = bundleAccess.Identifier.Text;
            string fieldName = bundleAccess.FieldName.Text;

            AbstractExpression boundExpression = BindExpression(syntax.RightExpr);
            // Get the instantiated bundle which is in a variable
            VariableSymbol? localVariable = BindVariableReference(bundleAccess.Identifier);

            if (localVariable is null)
            {
                return new AbstractErrorExpression(syntax);
            }

            BundleSymbol? bundleTemplate = GetBundleSymbol(localVariable);
            if (bundleTemplate is null)
            {
                _binderReport.ReportBundleDoesNotExist(bundleAccess.Identifier);
                return new AbstractErrorExpression(syntax);
            }

            AbstractBundleField? fieldToAccess = bundleTemplate.Fields.FirstOrDefault(f => f.FieldName == bundleAccess.FieldName.Text);
            if (fieldToAccess is null)
            {
                _binderReport.ReportFieldDoesNotExist(bundleAccess.FieldName);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractBundleFieldAssignment(syntax, localVariable, fieldToAccess, boundExpression);
        }

        private AbstractExpression BindBundleAccessExpression(BundleFieldAccessExpression syntax)
        {
            if (syntax.Identifier.IsMissing)
            {
                // This means the token was inserted by the parser and already reported the error
                return new AbstractErrorExpression(syntax);
            }

            VariableSymbol? variable = BindVariableReference(syntax.Identifier);
            if (variable == null)
            {
                return new AbstractErrorExpression(syntax);
            }

            if (variable.Type != TypeSymbol.Bundle)
            {
                _binderReport.ReportPrimitiveTypesDontHaveFields(variable.Name, syntax.Identifier.Location);
                return new AbstractErrorExpression(syntax);
            }

            BundleSymbol? bundleTemplate = GetBundleSymbol(variable);
            if (bundleTemplate is null)
            {
                _binderReport.ReportBundleDoesNotExist(syntax.Identifier);
                return new AbstractErrorExpression(syntax);
            }

            ImmutableArray<AbstractBundleField> fields = bundleTemplate.Fields;

            AbstractBundleField? fieldToAccess = fields.FirstOrDefault(f => f.FieldName == syntax.FieldName.Text);
            if (fieldToAccess is null)
            {
                _binderReport.ReportFieldDoesNotExist(syntax.FieldName);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractBundleFieldAccessExpression(syntax, variable, fieldToAccess);
        }

        private BundleSymbol? GetBundleSymbol(VariableSymbol variable)
        {
            if (variable.Type.IsNamedBundle)
            {
                if (variable.Type.Alias is null)
                {
                    throw new NullReferenceException(nameof(variable.Type.Alias));
                }

                if (variable.Type.Namespace is null)
                {
                    throw new NullReferenceException(nameof(variable.Type.Namespace));
                }

                if (_currentProgram.References.TryGetBundle(variable.Type.Namespace, variable.Type.Alias, out BundleSymbol? bundle))
                {
                    return bundle;
                }
            }

            return null;
        }

        #endregion

        #region Function Binding

        private FunctionSymbol BindFunctionDeclaration(FunctionDeclaration function)
        {
            ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            ImmutableArray<AttributeSymbol>.Builder attributes = ImmutableArray.CreateBuilder<AttributeSymbol>();
            HashSet<string> seenNames = new();

            foreach (Parameter parameter in function.Parameters)
            {
                string name = parameter.Identifier.Text;
                if (!seenNames.Add(name))
                {
                    _binderReport.ReportParameterAlreadyDeclared(parameter);
                    continue;
                }

                TypeSymbol parameterType = BindTypeClause(parameter.ParameterType);
                ParameterSymbol paramSymbol = new(name, parameterType, parameters.Count);
                parameters.Add(paramSymbol);
                if (!_currentScope.TryDeclareVariable(paramSymbol))
                {
                    // Fatal error
                    _binderReport.ReportCannotDeclareParameterVariable(parameter);
                }
            }

            TypeSymbol returnType = BindTypeClause(function.ReturnType) ?? TypeSymbol.Void;

            foreach (Token attribute in function.Attributes)
            {
                AttributeSymbol attributeSymbol = AttributeSymbol.FromName(attribute.Text);
                attributes.Add(attributeSymbol);

                if (!attributeSymbol.CanHaveParameters && function.Parameters.Count > 0)
                {
                    _binderReport.ReportAttributeDoesNotSupportFunctionParameters(attribute);
                    continue;
                }

                if (!attributeSymbol.CanHaveReturnType && returnType != TypeSymbol.Void)
                {
                    _binderReport.ReportAttributeDoesNotSupportFunctionReturnType(attribute);
                    continue;
                }
            }

            FunctionSymbol funcSymbol = new(function.Name.Text, parameters.ToImmutableArray(), attributes.ToImmutableArray(), returnType, function);
            if (function.Name.Text != null && !_currentScope.TryDeclareFunction(funcSymbol))
            {
                _binderReport.ReportBinderFunctionAlreadyExists(function);
            }

            return funcSymbol;
        }

        private FunctionSymbol BindNativeFunctionDeclaration(NativeFunctionDeclaration function)
        {
            ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            HashSet<string> seenNames = new();

            foreach (Parameter parameter in function.Parameters)
            {
                string name = parameter.Identifier.Text;
                if (!seenNames.Add(name))
                {
                    _binderReport.ReportParameterAlreadyDeclared(parameter);
                    continue;
                }

                TypeSymbol parameterType = BindTypeClause(parameter.ParameterType);
                ParameterSymbol paramSymbol = new(name, parameterType, parameters.Count);
                parameters.Add(paramSymbol);
            }

            TypeSymbol type = BindTypeClause(function.ReturnType) ?? TypeSymbol.Void;
            FunctionSymbol funcSymbol = new(function.Name.Text, parameters.ToImmutableArray(), [], type, function);
            if (function.Name.Text != null && !_currentScope.TryDeclareNativeFunction(funcSymbol))
            {
                _binderReport.ReportBinderFunctionAlreadyExists(function);
            }

            return funcSymbol;
        }

        #endregion

        #region Expression Binding

        private AbstractExpression BindExpression(Expression expr, bool canBeVoid = false)
        {
            AbstractExpression result = BindExpressionInternal(expr);
            if (!canBeVoid && result.ResultType == TypeSymbol.Void)
            {
                _binderReport.ReportExpresionMustHaveValue(expr);
                return new AbstractErrorExpression(expr);
            }

            return result;
        }

        private AbstractExpression BindExpressionInternal(Expression expr) => expr.Type switch
        {
            NodeType.ParenthesizedExpression => BindParanthesizedExpression((ParenthesizedExpression)expr),
            NodeType.LiteralExpression => BindLiteralExpression((LiteralExpression)expr),
            NodeType.UnaryExpression => BindUnaryExpression((UnaryExpression)expr),
            NodeType.BinaryExpression => BindBinaryExpression((BinaryExpression)expr),
            NodeType.NameExpression => BindNameExpression((NameExpression)expr),
            NodeType.BundleFieldAccessExpression => BindBundleAccessExpression((BundleFieldAccessExpression)expr),
            NodeType.AssignmentExpression => BindAssignmentExpression((AssignmentExpression)expr),
            NodeType.CallExpression => BindCallExpression((CallExpression)expr),
            _ => throw new Exception($"Unexpected syntax '{expr.Type}'"),
        };
        private AbstractExpression BindParanthesizedExpression(ParenthesizedExpression syntax) => BindExpression(syntax.Expression);

        private AbstractExpression BindAssignmentExpression(AssignmentExpression expr)
        {
            if (expr.Identifier is BundleFieldAccessExpression)
            {
                return BindBundleFieldAssignmentExpression(expr);
            }

            NameExpression nameExpression = (NameExpression)expr.Identifier;

            string? name = nameExpression.Identifier.Text;
            AbstractExpression boundExpression = BindExpression(expr.RightExpr);

            VariableSymbol? variable = BindVariableReference(nameExpression.Identifier);
            if (variable == null)
            {
                return boundExpression;
            }

            if (variable.IsReadOnly)
                _binderReport.ReportCannotAssign(expr.Operator.Location, name);

            AbstractExpression convertedExpression;
            if (expr.Operator.Type != NodeType.EqualsToken)
            {
                NodeType equivalentOperatorTokenKind = SyntaxEx.GetBinaryOperatorOfAssignmentOperator(expr.Operator.Type);
                AbstractBinaryOperator? boundOperator = AbstractBinaryOperator.Bind(equivalentOperatorTokenKind, variable.Type, boundExpression.ResultType);

                if (boundOperator == null)
                {
                    _binderReport.ReportUndefinedBinaryOperator(expr.Operator.Location, expr.Operator.Text, variable.Type, boundExpression.ResultType);
                    return new AbstractErrorExpression(expr);
                }

                convertedExpression = BindConversion(expr.RightExpr.Location, boundExpression, variable.Type);
                return new AbstractCompoundAssignmentExpression(expr, variable, boundOperator, convertedExpression);
            }

            convertedExpression = BindConversion(expr.RightExpr.Location, boundExpression, variable.Type);
            return new AbstractAssignmentExpression(expr, variable, convertedExpression);
        }

        private static AbstractLiteralExpression BindLiteralExpression(LiteralExpression syntax)
        {
            object value = syntax.Value ?? 0;
            return new AbstractLiteralExpression(syntax, value);
        }

        private AbstractExpression BindUnaryExpression(UnaryExpression syntax)
        {
            AbstractExpression boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.ResultType == TypeSymbol.Error)
                return new AbstractErrorExpression(syntax);

            AbstractUnaryOperator? boundOperator = AbstractUnaryOperator.Bind(syntax.Operator.Type, boundOperand.ResultType);

            if (boundOperator is null)
            {
                _binderReport.ReportUndefinedUnaryOperator(syntax.Operator.Location, syntax.Operator.Text, boundOperand.ResultType);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractUnaryExpression(syntax, boundOperator, boundOperand);
        }

        private AbstractExpression BindBinaryExpression(BinaryExpression syntax)
        {
            AbstractExpression boundLeft = BindExpression(syntax.Left);
            AbstractExpression boundRight = BindExpression(syntax.Right);

            if (boundLeft.ResultType == TypeSymbol.Error || boundRight.ResultType == TypeSymbol.Error)
                return new AbstractErrorExpression(syntax);

            AbstractBinaryOperator? BoundOperatorType = AbstractBinaryOperator.Bind(syntax.Operator.Type, boundLeft.ResultType, boundRight.ResultType);
            if (BoundOperatorType is null)
            {
                _binderReport.ReportUndefinedBinaryOperator(syntax.Operator.Location, syntax.Operator.Text, boundLeft.ResultType, boundRight.ResultType);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractBinaryExpression(syntax, boundLeft, BoundOperatorType, boundRight);
        }

        private AbstractExpression BindNameExpression(NameExpression syntax)
        {
            if (syntax.Identifier.IsMissing)
            {
                // This means the token was inserted by the parser and already reported the error
                return new AbstractErrorExpression(syntax);
            }

            VariableSymbol? variable = BindVariableReference(syntax.Identifier);
            if (variable == null)
                return new AbstractErrorExpression(syntax);

            return new AbstractVariableExpression(syntax, variable);
        }

        private AbstractExpression BindCallExpression(CallExpression expr)
        {
            // This is for casting of type to type, as it's treated as a function call
            if (expr.Arguments.Count == 1 && LookupType(expr.Identifier.Text) is TypeSymbol type)
                return BindConversion(expr.Arguments[0], type, allowExplicit: true);

            // We lookup into references too, if nothinng is found we assume it doesn't exist even if we could technically compile just fine
            FunctionSymbol? function = TryLookupFunctionSymbol(expr);
            if (function == null)
            {
                return new AbstractErrorExpression(expr);
            }

            if (!AreFunctionCallArgumentOk(expr, function))
            {
                return new AbstractErrorExpression(expr);
            }

            BindFunctionCallArguments(expr, out ImmutableArray<AbstractExpression> boundArguments, function);
            return new AbstractCallExpression(expr, expr.IsAsyncCall, expr.Namespace?.Text, function, boundArguments);
        }

        private FunctionSymbol? TryLookupFunctionSymbol(CallExpression callExpression)
        {
            if (callExpression.Namespace != null && callExpression.Namespace.Text != _currentProgram.Namespace.Text)
            {
                if (_currentProgram.References.TryGetFunction(callExpression.Namespace.Text, callExpression.Identifier.Text, out FunctionSymbol? function))
                {
                    return function;
                }

                _binderReport.ReportUndefinedFunction(callExpression.Identifier.Location, callExpression.Identifier.Text);
                return null;
            }

            Symbol? symbol = _currentScope.TryLookupSymbol(callExpression.Identifier.Text);
            if (symbol is not FunctionSymbol localFunction)
            {
                _binderReport.ReportNotAFunction(callExpression.Identifier.Location, callExpression.Identifier.Text);
                return null;
            }

            return localFunction;
        }

        private void BindFunctionCallArguments(CallExpression expr, out ImmutableArray<AbstractExpression> boundArguments, FunctionSymbol function)
        {
            ImmutableArray<AbstractExpression>.Builder? boundArgumentsBuilder = ImmutableArray.CreateBuilder<AbstractExpression>();
            foreach (Expression? argument in expr.Arguments)
            {
                AbstractExpression? boundArgument = BindExpression(argument);
                boundArgumentsBuilder.Add(boundArgument);
            }

            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                TextLocation argumentLocation = expr.Arguments[i].Location;
                AbstractExpression argument = boundArgumentsBuilder[i];
                ParameterSymbol? parameter = function.Parameters[i];
                boundArgumentsBuilder[i] = BindConversion(argumentLocation, argument, parameter.Type);
            }

            boundArguments = boundArgumentsBuilder.ToImmutableArray();
        }

        private bool AreFunctionCallArgumentOk(CallExpression expression, FunctionSymbol function)
        {
            if (expression.Arguments.Count != function.Parameters.Length)
            {
                TextSpan span;
                if (expression.Arguments.Count > function.Parameters.Length)
                {
                    Node firstExceedingNode;
                    if (function.Parameters.Length > 0)
                        firstExceedingNode = expression.Arguments.GetSeparator(function.Parameters.Length - 1);
                    else
                        firstExceedingNode = expression.Arguments[0];

                    Expression? lastExceedingArgument = expression.Arguments[^1];
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = expression.CloseParenthesis.Span;
                }
                TextLocation location = new(expression.SourceCode, span);
                _binderReport.ReportWrongNumberOfArguments(location, function.Name, function.Parameters.Length, expression.Arguments.Count);
                return false;
            }

            return true;
        }

        #endregion

        #region Statement Binding
        private AbstractStatement BindStatement(Statement statement)
        {
            AbstractStatement result = BindStatementInternal(statement);

            if (result is AbstractExpressionStatement es)
            {
                bool isAllowedExpression = es.Expression.Type == AbstractNodeType.ErrorExpression ||
                                            es.Expression.Type == AbstractNodeType.AssignmentExpression ||
                                            es.Expression.Type == AbstractNodeType.BundleFieldAssignmentExpression ||
                                            es.Expression.Type == AbstractNodeType.CallExpression ||
                                            es.Expression.Type == AbstractNodeType.CompoundAssignmentExpression;
                if (!isAllowedExpression)
                {
                    _binderReport.ReportInvalidExpressionStatement(statement.Location);
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            return result;
        }

        private AbstractStatement BindStatementInternal(Statement syntax) => syntax.Type switch
        {
            NodeType.BlockStatement => BindBlockStatement((BlockStatement)syntax),
            NodeType.ExpressionStatement => BindExpressionStatement((ExpressionStatement)syntax),
            NodeType.WaitStatement => BindWaitStatement((WaitStatement)syntax),
            NodeType.IfStatement => BindIfStatement((IfStatement)syntax),
            NodeType.WhileStatement => BindWhileStatement((WhileStatement)syntax),
            NodeType.DoWhileStatement => BindDoWhileStatement((DoWhileStatement)syntax),
            NodeType.ForStatement => BindForStatement((ForStatement)syntax),
            NodeType.BreakStatement => BindBreakStatement((BreakStatement)syntax),
            NodeType.ContinueStatement => BindContinueStatement((ContinueStatement)syntax),
            NodeType.ReturnStatement => BindReturnStatement((ReturnStatement)syntax),
            NodeType.VariableDeclarationCollection => BindVariableDeclarations((VariableDeclarationCollection)syntax),
            _ => throw new Exception($"Unexpected syntax '{syntax.Type}'"),
        };

        private static AbstractExpressionStatement BindErrorStatement(Node syntax) => new(syntax, new AbstractErrorExpression(syntax));


        private AbstractStatement BindBreakStatement(BreakStatement syntax)
        {
            if (_loopStack.Count == 0)
            {
                _binderReport.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement(syntax);
            }

            AbstractLabel breakLabel = _loopStack.Peek().BreakLabel;
            return new AbstractGotoStatement(syntax, breakLabel);
        }
        private AbstractStatement BindContinueStatement(ContinueStatement syntax)
        {
            if (_loopStack.Count == 0)
            {
                _binderReport.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement(syntax);
            }

            AbstractLabel continueLabel = _loopStack.Peek().ContinueLabel;
            return new AbstractGotoStatement(syntax, continueLabel);
        }

        private AbstractReturnStatement BindReturnStatement(ReturnStatement syntax)
        {
            // Does the function have a return type?
            // Does the return type match?

            AbstractExpression? expression = syntax.Expression is null ? null : BindExpression(syntax.Expression);
            FunctionSymbol? currentFunction = _currentFunction;
            Debug.Assert(_currentFunction != null);
            // If a function does not return anything then we shouldn't be here to begin with.
            if (currentFunction!.ReturnType == TypeSymbol.Void)
            {
                if (expression is not null)
                    _binderReport.ReportInvalidReturnExpression(syntax.Expression!.Location, currentFunction.Name);
            }
            else
            {
                // A return statement always needs an expression if return type is not void
                if (expression is null)
                    _binderReport.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, currentFunction.Name, currentFunction.ReturnType);
                else
                    expression = BindConversion(syntax.Expression!.Location, expression, currentFunction.ReturnType);
            }

            return new AbstractReturnStatement(syntax, expression);
        }

        private AbstractBlockStatement BindBlockStatement(BlockStatement node)
        {
            ImmutableArray<AbstractStatement>.Builder statements = ImmutableArray.CreateBuilder<AbstractStatement>();

            //Block of codes have a new scope
            _currentScope = new(_currentScope);
            foreach (Statement? s in node.Statements)
            {
                AbstractStatement? statement = BindStatement(s);
                statements.Add(statement);
            }
            _currentScope = _currentScope.Parent!;

            return new AbstractBlockStatement(node, statements.ToImmutable());
        }

        private AbstractNamespace BindNamespaceStatement(NamespaceStatement @namespace)
        {
            _currentScope.Namespace = new(@namespace);
            return _currentScope.Namespace;
        }

        private AbstractExpressionStatement BindExpressionStatement(ExpressionStatement node)
        {
            AbstractExpression expression = BindExpression(node.Expression, true);
            return new AbstractExpressionStatement(node, expression);
        }

        private AbstractStatement BindWaitStatement(WaitStatement expr)
        {
            AbstractExpression timeExpr = BindExpression(expr.Time, canBeVoid: false);
            if (timeExpr is not AbstractErrorExpression && timeExpr.ResultType != TypeSymbol.Int)
            {
                _binderReport.ReportWaitMustBeNumber(timeExpr, TypeSymbol.Int);
                timeExpr = new AbstractErrorExpression(timeExpr.OriginalNode);
            }

            return new AbstractWaitStatement(expr, timeExpr);
        }

        private AbstractStatement BindIfStatement(IfStatement syntax)
        {
            AbstractExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if ((bool)condition.ConstantValue.Value == false)
                    _binderReport.ReportUnreachableCode(syntax.ThenStatement);
                else if (syntax.ElseClause != null)
                    _binderReport.ReportUnreachableCode(syntax.ElseClause.ElseStatement);
            }

            AbstractStatement thenStatement = BindStatement(syntax.ThenStatement);
            AbstractStatement? elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new AbstractIfStatement(syntax, condition, thenStatement, elseStatement);
        }

        #endregion

        [return: NotNullIfNotNull(nameof(typeClause))]
        private TypeSymbol? BindTypeClause(TypeClause? typeClause)
        {
            if (typeClause is null)
                return null;

            TypeSymbol? type = null;
            string typeName = typeClause.Identifier.Text;
            if (typeClause.Namespace != null && typeClause.Namespace.Text != _currentProgram.Namespace.Text)
            {
                if (_currentProgram.References.TryGetBundle(typeClause.Namespace.Text, typeName, out BundleSymbol? _))
                {
                    return TypeSymbol.GetNamedBundleType(typeClause.Namespace.Text, typeName);
                }
            }
            else
            {
                type = LookupType(typeName);
            }

            // Null means we are trying to use a bundle type or type actually does not exist
            if (type is null)
            {
                // Check if this is a user defined bundle
                if (_currentUnit.Bundles.Any(b => b.Name.Text == typeName))
                {
                    return TypeSymbol.GetNamedBundleType(_currentProgram.Namespace.Text, typeName);
                }

                _binderReport.ReportUndefinedType(typeClause.Identifier);
                return TypeSymbol.Error;
            }

            return type!;
        }

        public static TypeSymbol? LookupType(string name) => name switch
        {
            "bool" => TypeSymbol.Bool,
            "int" => TypeSymbol.Int,
            "float" => TypeSymbol.Float,
            "string" => TypeSymbol.String,
            //"any" => TypeSymbol.Any,
            "void" => TypeSymbol.Void,
            "bundle" => TypeSymbol.Bundle,
            _ => null,
        };
    }
}
