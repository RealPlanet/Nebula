using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Expression.Bundles;
using Nebula.Core.Compilation.AST.Tree.Operators;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow;
using Nebula.Core.Compilation.AST.Tree.Statements.Loop;
using Nebula.Core.Compilation.CST.Tree;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Declaration;
using Nebula.Core.Compilation.CST.Tree.Declaration.Bundle;
using Nebula.Core.Compilation.CST.Tree.Declaration.Function;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Core.Compilation.CST.Tree.Statements;
using Nebula.Core.Compilation.CST.Tree.Types;
using Nebula.Core.Compilation.Lowering;
using Nebula.Core.Graph;
using Nebula.Core.Reporting;
using Nebula.Core.Utility.Abstract;
using Nebula.Interop.Enumerators;
using Nebula.Interop.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nebula.Core.Compilation.AST.Binding
{
    public sealed class Binder
    {
        private Binder(ICollection<CompilationUnit> units, ICollection<Script> references)
        {
            _allUnitsToBind = units.ToList();
            _allScriptToReference = references.ToList();
        }

        private readonly List<CompilationUnit> _allUnitsToBind;
        private readonly List<Script> _allScriptToReference;
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

        public static ICollection<AbstractProgram> Bind(ICollection<CompilationUnit> units, ICollection<Script> references, out Report bindingReport)
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

                CreateStaticInitializer();
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

                    Script? scriptReference = _allScriptToReference.FirstOrDefault(s => s.Namespace == import.Namespace);
                    if (scriptReference != null)
                    {
                        _currentProgram.References.AddScriptReference(scriptReference);
                        continue;
                    }

                    _binderReport.PushError($"Import '{import.Namespace}' not found!");
                }

                // Now that all types have been declared we can bind the function declarations
                foreach (NativeFunctionDeclaration function in _currentUnit.NativeFunctions)
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

                foreach (var kvp in _currentProgram.Functions)
                {
                    FunctionSymbol declaration = kvp.Key;
                    _currentFunction = declaration;
                    // Use the function scope to correctly retrieve parameters
                    _currentScope = declaration.FunctionScope;

                    AbstractStatement body;
                    // Typically this is always false but if we auto generate a function in the binder we don't want to process it here again
                    if (kvp.Value != null)
                    {
                        body = kvp.Value;
                    }
                    else
                    {
                        body = BindBlockStatement(declaration.Declaration!.Body, createNewScope: false);
                    }

                    AbstractBlockStatement loweredBody = lowerer.Lower(declaration, body);
                    if (declaration.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    {
                        if (declaration.Declaration != null)
                        {
                            _binderReport.ReportAllPathsMustReturn(declaration.Declaration.Name.Text, declaration.Declaration.SignatureLocation);
                        }
                        else
                        {
                            _binderReport.ReportAllPathsMustReturn(declaration.Name, default);
                        }
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

        private void CreateStaticInitializer()
        {
            foreach (var global in _currentUnit.Globals)
            {
                var globalDefinition = BindVariableDeclarations(global, _currentProgram.Namespace.Text);
                foreach (var variable in globalDefinition.AllVariables)
                {
                    _currentProgram.Globals.Add(variable.Variable, variable);
                }
            }

            if (_currentUnit.Globals.Count > 0)
            {
                Scope functionScope = new Scope(_currentScope);

                const string ctorName = "0__script_static_ctor__0";
                FunctionSymbol funcSymbol = new(ctorName,
                                                [],
                                                [AttributeSymbol.FromEnum(AttributeType.AutoGenerated), AttributeSymbol.FromEnum(AttributeType.AutoExec), AttributeSymbol.FromEnum(AttributeType.Initializer)],
                                                TypeSymbol.Void,
                                                functionScope,
                                                null);

                // Ignore this error so we can bind everything else and report those errors too
                if (funcSymbol.Name != null &&
                    !_currentScope.TryDeclareFunction(funcSymbol))
                {
                    _binderReport.ReportInternalErrorCouldNotDeclareStaticConstructor(ctorName);
                }

                var statements = new List<AbstractStatement>();
                foreach (var kvp in _currentProgram.Globals)
                {
                    var variable = kvp.Value;
                    if (variable.Initializer is AbstractDeclarationAssignmentExpression declarationAssignment)
                    {
                        var initializer = declarationAssignment.Expression;
                        var statement = new AbstractExpressionStatement(variable.OriginalNode,
                                                                        AbstractNodeFactory.Assignment(variable.OriginalNode, variable.Variable, initializer));

                        statements.Add(statement);
                    }
                }

                AbstractBlockStatement initializerBody = AbstractNodeFactory.Block(statements.First().OriginalNode, statements.ToArray());
                _currentProgram.Functions.Add(funcSymbol, initializerBody);
            }
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

            if (expression.Type == AbstractNodeType.ObjectInitializationExpression
                && expression.ResultType == TypeSymbol.BaseObject)
            {
                ((AbstractObjectInitializationExpression)expression).SetAllocationResult(type);
                return expression;
            }

            if (expression.Type == AbstractNodeType.ArrayInitializationExpression
                && expression.ResultType == TypeSymbol.BaseArray)
            {
                ((AbstractArrayInitializationExpression)expression).SetAllocationResult(type);
                return expression;
            }

            if (!conversion.Exists)
            {
                if (expression.ResultType != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    _binderReport.ReportCannotConvertType(reportLocation, expression.ResultType, type);
                }

                return new AbstractErrorExpression(expression.OriginalNode);
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _binderReport.ReportCannotConvertTypeImplicity(reportLocation, expression.ResultType, type);
                return new AbstractErrorExpression(expression.OriginalNode);
            }

            if (conversion.IsIdentity)
            {
                return expression;
            }

            return new AbstractConversionExpression(expression.OriginalNode, type, expression);
        }

        #endregion

        #region Variable Binding

        /// <summary>
        /// Bind an CST Variable declaration
        /// </summary>
        private AbstractVariableDeclarationCollection BindVariableDeclarations(VariableDeclarationCollection node, string @namespace)
        {
            bool isReadOnly = node.IsConst;

            ImmutableArray<AbstractVariableDeclaration>.Builder boundVariables = ImmutableArray.CreateBuilder<AbstractVariableDeclaration>();
            foreach (VariableDeclaration declaration in node.Declarations)
            {
                AbstractVariableDeclaration abstractDeclaration = BindVariableDeclaration(@namespace, isReadOnly, declaration);
                boundVariables.Add(abstractDeclaration);
            }

            return new AbstractVariableDeclarationCollection(node, boundVariables.ToImmutableArray());
        }

        private AbstractVariableDeclaration BindVariableDeclaration(string @namespace, bool isReadOnly, VariableDeclaration declaration)
        {
            if (declaration.AssignmentExpression.Identifier is not NameExpression nameExpr)
            {
                throw new NotSupportedException();
            }

            TypeSymbol type = BindTypeClause(declaration.VarType);

            VariableSymbol variable = BindVariableDeclaration(@namespace,
                                                          nameExpr,
                                                          isReadOnly,
                                                          type,
                                                          null);

            var boundAssignment = BindAssignmentExpression(declaration.AssignmentExpression, parentExpression: null, isDeclarationAssignment: true);
            if (boundAssignment is AbstractDeclarationAssignmentExpression declAssignment)
            {
                variable.SetConstant(declAssignment.Expression.ConstantValue);
            }

            return new AbstractVariableDeclaration(declaration, variable, boundAssignment);
        }

        /// <summary> Try to declare this variable within the scope </summary>
        private VariableSymbol BindVariableDeclaration(string @namespace, NameExpression identifier, bool isReadOnly, TypeSymbol type, AbstractConstant? constant = null)
        {
            string? name = identifier.Identifier.Text ?? "?";
            bool isGlobal = !string.IsNullOrEmpty(@namespace);
            VariableSymbol variable = isGlobal ?
                new GlobalVariableSymbol(@namespace, name, isReadOnly, type, constant) :
                new LocalVariableSymbol(name, isReadOnly, type, constant);

            // Should never happen as shadowing is allowed and we created a new scope
            if (!identifier.Identifier.IsMissing && !_currentScope.TryDeclareVariable(variable))
            {
                _binderReport.ReportVariableAlreadyDeclared(identifier.Identifier);
            }

            return variable;
        }

        private VariableSymbol? BindVariableReference(NameExpression nameExpression)
        {
            string? ns = nameExpression.Namespace?.Text;
            string name = nameExpression.Identifier.Text;
            return BindVariableReference(ns, name, nameExpression.Location);
        }

        private VariableSymbol? BindVariableReference(string? @namespace, string name, TextLocation location)
        {
            if (!string.IsNullOrEmpty(@namespace))
            {
                if (_currentProgram.References.TryGetGlobalVariable(@namespace, name, out var symbol))
                {
                    return symbol;
                }

                throw new NotImplementedException("Report this error");
            }

            switch (_currentScope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    _binderReport.ReportUndefinedVariable(location, name);
                    return null;

                default:
                    _binderReport.ReportNotAVariable(location, name);
                    return null;
            }
        }

        #endregion

        #region Control-Flow Loops

        private AbstractWhileStatement BindWhileStatement(WhileStatement syntax)
        {
            AbstractExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null
                && condition.ConstantValue.Value != null)
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

        private AbstractForStatement BindForStatement(ForStatement syntax)
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

        private AbstractExpression BindArrayAssignmentExpression(AssignmentExpression syntax, ArrayAccessExpression arrayAccess)
        {
            // Get the array which is in a variable
            VariableSymbol? variable = BindVariableReference(arrayAccess);
            if (variable is null)
            {
                return new AbstractErrorExpression(syntax);
            }

            AbstractExpression expressionToAssign = BindExpression(syntax.RightExpr);
            if (syntax.Operator.Type != NodeType.EqualsToken)
            {
                _binderReport.ReportUndefinedBinaryOperator(syntax.Operator.Location, syntax.Operator.Text, variable.Type, expressionToAssign.ResultType);
                return new AbstractErrorExpression(syntax);
            }

            AbstractExpression indexExpression = BindExpression(arrayAccess.AccessExpression);
            if (indexExpression.ResultType != TypeSymbol.Int)
            {
                _binderReport.ReportCannotConvertType(indexExpression.OriginalNode.Location, indexExpression.ResultType, TypeSymbol.Int);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractArrayAssignmentExpression(syntax, variable, indexExpression, expressionToAssign);
        }

        private AbstractExpression BindObjectFieldAccess(ObjectFieldAccess syntax, AbstractExpression? parentFieldAccess)
        {
            if (syntax.Identifier.IsMissing
                || parentFieldAccess is null)
            {
                // This means the token was inserted by the parser and already reported the error
                return new AbstractErrorExpression(syntax);
            }

            var bundleTemplate = GetBundleSymbol(parentFieldAccess.ResultType);
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

            return new AbstractObjectFieldAccessExpression(syntax, fieldToAccess);
        }

        private AbstractExpression BindArrayAccessExpression(ArrayAccessExpression syntax)
        {
            if (syntax.Identifier.IsMissing)
            {
                // This means the token was inserted by the parser and already reported the error
                return new AbstractErrorExpression(syntax);
            }


            VariableSymbol? variable = BindVariableReference(syntax);
            if (variable == null)
            {
                return new AbstractErrorExpression(syntax);
            }

            if (!variable.Type.IsArray)
            {
                _binderReport.ReportVariableNotOfType(variable.Name, TypeSymbol.BaseArray, syntax.Location);
                return new AbstractErrorExpression(syntax);
            }

            AbstractExpression indexExpression = BindExpression(syntax.AccessExpression);
            if (indexExpression.ResultType != TypeSymbol.Int)
            {
                _binderReport.ReportCannotConvertType(indexExpression.OriginalNode.Location, indexExpression.ResultType, TypeSymbol.Int);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractArrayAccessExpression(syntax, variable, indexExpression);
        }

        private BundleSymbol? GetBundleSymbol(TypeSymbol type)
        {
            if (type is ObjectTypeSymbol objType)
            {
                if (objType.Name is null)
                {
                    throw new NullReferenceException(nameof(objType.Name));
                }

                if (objType.Namespace is null)
                {
                    throw new NullReferenceException(nameof(objType.Namespace));
                }

                if (_currentProgram.References.TryGetBundle(objType.Namespace, objType.Name, out BundleSymbol? bundle))
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

            Scope functionScope = new Scope(_currentScope);

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
                if (!functionScope.TryDeclareVariable(paramSymbol))
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

            FunctionSymbol funcSymbol = new(function.Name.Text, parameters.ToImmutableArray(), attributes.ToImmutableArray(), returnType, functionScope, function);
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
            FunctionSymbol funcSymbol = new(function.Name.Text, parameters.ToImmutableArray(), [], type, null!, function);
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
            Debug.Assert(result is not null);
            if (!canBeVoid && result.ResultType == TypeSymbol.Void)
            {
                _binderReport.ReportExpresionMustHaveValue(expr);
                return new AbstractErrorExpression(expr);
            }

            return result;
        }

        private AbstractExpression BindRightExpression(Expression right, AbstractExpression boundLeft) => right.Type switch
        {
            NodeType.ObjectFieldAccessExpression => BindObjectFieldAccess((ObjectFieldAccess)right, parentFieldAccess: boundLeft),
            NodeType.NameExpression => BindNameExpression((NameExpression)right, boundLeft),
            NodeType.AssignmentExpression => BindAssignmentExpression((AssignmentExpression)right, boundLeft, isDeclarationAssignment: false),
            _ => BindExpression(right),
        };

        private AbstractExpression BindExpressionInternal(Expression expr) => expr.Type switch
        {
            NodeType.ParenthesizedExpression => BindParanthesizedExpression((ParenthesizedExpression)expr),
            NodeType.LiteralExpression => BindLiteralExpression((LiteralExpression)expr),
            NodeType.UnaryExpression => BindUnaryExpression((UnaryExpression)expr),
            NodeType.BinaryExpression => BindBinaryExpression((BinaryExpression)expr),
            NodeType.NameExpression => BindNameExpression((NameExpression)expr, parentExpression: null),
            NodeType.ArrayAccessExpression => BindArrayAccessExpression((ArrayAccessExpression)expr),
            NodeType.ObjectFieldAccessExpression => BindObjectFieldAccess((ObjectFieldAccess)expr, parentFieldAccess: null),
            NodeType.AssignmentExpression => BindAssignmentExpression((AssignmentExpression)expr, null, false),
            NodeType.CallExpression => BindCallExpression((CallExpression)expr),
            NodeType.ObjectCallExpression => BindObjectCallExpression((ObjectCallExpression)expr),
            NodeType.ArrayInitializationExpression => BindArrayInitializationExpression((ArrayInitializationExpression)expr),
            NodeType.ObjectInitializationExpression => BindObjectInitializationExpression((ObjectInitializationExpression)expr),
            NodeType.IsDefinedExpression => BindIsDefinedExpression((IsDefinedExpression)expr),
            _ => throw new Exception($"Unexpected syntax '{expr.Type}'"),
        };

        private AbstractExpression BindParanthesizedExpression(ParenthesizedExpression syntax) => BindExpression(syntax.Expression);

        private AbstractExpression BindAssignmentExpression(AssignmentExpression assignmentExpression, AbstractExpression? parentExpression, bool isDeclarationAssignment)
        {
            switch (assignmentExpression.Identifier.Type)
            {
                case NodeType.ArrayAccessExpression:
                    {
                        Debug.Assert(isDeclarationAssignment == false);
                        return BindArrayAssignmentExpression(assignmentExpression, (ArrayAccessExpression)assignmentExpression.Identifier);
                    }
                    //case NodeType.ObjectVariableAccessExpression:
                    //    return BindObjectVariableAccessExpression((ObjectVariableAccessExpression)assignmentExpression.Identifier);
            }

            AbstractExpression boundInitializer = BindExpression(assignmentExpression.RightExpr);

            switch (parentExpression)
            {
                case AbstractBinaryExpression binaryExpression:
                    {
                        Debug.Assert(isDeclarationAssignment == false);
                        Debug.Assert(binaryExpression.Right.Type == AbstractNodeType.ObjectFieldAccessExpression);
                        var fieldAccess = (AbstractObjectFieldAccessExpression)binaryExpression.Right;
                        AbstractExpression? convertedInitializer = BindConversion(assignmentExpression.RightExpr.Location, boundInitializer, fieldAccess.Field.FieldType);
                        if (!PostProcessAssignmentExpression(convertedInitializer))
                        {
                            return new AbstractErrorExpression(assignmentExpression.RightExpr);
                        }

                        return new AbstractObjectFieldAssignmentExpression(assignmentExpression, binaryExpression, fieldAccess.Field, convertedInitializer);
                    }
            }

            AbstractExpression identifierExpression = BindExpression(assignmentExpression.Identifier);

            switch (identifierExpression)
            {
                case AbstractBinaryExpression binaryExpression:
                    {
                        Debug.Assert(isDeclarationAssignment == false);
                        Debug.Assert(binaryExpression.Right.Type == AbstractNodeType.ObjectFieldAccessExpression);
                        var fieldAccess = (AbstractObjectFieldAccessExpression)binaryExpression.Right;
                        AbstractExpression? convertedInitializer = BindConversion(assignmentExpression.RightExpr.Location, boundInitializer, fieldAccess.Field.FieldType);
                        if (!PostProcessAssignmentExpression(convertedInitializer))
                        {
                            return new AbstractErrorExpression(assignmentExpression.RightExpr);
                        }

                        fieldAccess.Mode = AbstractObjectFieldAccessExpression.FieldMode.Write;
                        return new AbstractObjectFieldAssignmentExpression(assignmentExpression, binaryExpression, fieldAccess.Field, convertedInitializer);
                    }
            }

            VariableSymbol variable;
            switch (identifierExpression)
            {
                case AbstractVariableExpression variableExpression:
                    {
                        variable = variableExpression.Variable;
                        break;
                    }
                case AbstractErrorExpression errorExpression:
                    {
                        return new AbstractErrorExpression(assignmentExpression);
                    }
                default:
                    {
                        if (assignmentExpression.Operator.Type == NodeType.EqualsToken)
                        {
                            _binderReport.ReportAssignmentLeftHandSideNotValid(assignmentExpression.Identifier.Location);
                            return new AbstractErrorExpression(assignmentExpression);
                        }

                        throw new NotSupportedException();
                    }
            }

            if (variable.IsReadOnly && variable.Constant != null)
            {
                _binderReport.ReportCannotAssign(assignmentExpression.Operator.Location, variable.Name);
            }

            AbstractExpression convertedExpression;
            if (assignmentExpression.Operator.Type != NodeType.EqualsToken)
            {
                NodeType equivalentOperatorTokenKind = SyntaxEx.GetBinaryOperatorOfAssignmentOperator(assignmentExpression.Operator.Type);
                AbstractBinaryOperator? boundOperator = AbstractBinaryOperator.Bind(equivalentOperatorTokenKind, variable.Type, boundInitializer.ResultType);

                if (boundOperator == null)
                {
                    _binderReport.ReportUndefinedBinaryOperator(assignmentExpression.Operator.Location, assignmentExpression.Operator.Text, variable.Type, boundInitializer.ResultType);
                    return new AbstractErrorExpression(assignmentExpression);
                }

                convertedExpression = BindConversion(assignmentExpression.RightExpr.Location, boundInitializer, variable.Type);
                return new AbstractCompoundAssignmentExpression(assignmentExpression, variable, boundOperator, convertedExpression);
            }

            convertedExpression = BindConversion(assignmentExpression.RightExpr.Location, boundInitializer, variable.Type);

            if (!PostProcessAssignmentExpression(convertedExpression))
            {
                return new AbstractErrorExpression(assignmentExpression.RightExpr);
            }

            if (isDeclarationAssignment)
            {
                return new AbstractDeclarationAssignmentExpression(assignmentExpression, variable, convertedExpression);
            }

            return new AbstractAssignmentExpression(assignmentExpression, variable, convertedExpression);
        }

        private bool PostProcessAssignmentExpression(AbstractExpression convertedExpression)
        {
            switch (convertedExpression.Type)
            {
                case AbstractNodeType.ObjectInitializationExpression:
                    {
                        AbstractObjectInitializationExpression oie = (AbstractObjectInitializationExpression)convertedExpression;
                        var bundleType = GetBundleSymbol(oie.ResultType);
                        if (bundleType is null)
                        {
                            _binderReport.ReportBundleDoesNotExist(oie.ResultType.Name, oie.OriginalNode.Location);
                            return false;
                        }

                        foreach (var fieldInitializationExpression in oie.FieldExpressions)
                        {
                            AbstractBundleField? fieldToInitialize = bundleType.Fields.FirstOrDefault(f => f.FieldName == fieldInitializationExpression.FieldName);
                            if (fieldToInitialize is null)
                            {
                                _binderReport.ReportFieldDoesNotExist(((ObjectFieldInitializationExpression)fieldInitializationExpression.OriginalNode).Identifier);
                                continue;
                            }

                            fieldInitializationExpression.SetFieldToInitialize(fieldToInitialize);
                            // If it returns an object we need to recursively initialize the object
                            if (fieldInitializationExpression.ResultType.IsObject)
                            {
                                var subInitializer = (AbstractObjectInitializationExpression)fieldInitializationExpression.Initializer;
                                PostProcessAssignmentExpression(subInitializer);
                            }
                        }
                        break;
                    }
            }

            return true;
        }

        private static AbstractLiteralExpression BindLiteralExpression(LiteralExpression syntax)
        {
            object? value = syntax.Value;
            return new AbstractLiteralExpression(syntax, value);
        }

        private AbstractExpression BindUnaryExpression(UnaryExpression syntax)
        {
            AbstractExpression boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.ResultType == TypeSymbol.Error)
            {
                return new AbstractErrorExpression(syntax);
            }

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
            AbstractExpression boundRight = BindRightExpression(syntax.Right, boundLeft);

            if (boundLeft.ResultType == TypeSymbol.Error || boundRight.ResultType == TypeSymbol.Error)
            {
                return new AbstractErrorExpression(syntax);
            }

            AbstractBinaryOperator? boundOperatorType;
            if (syntax.Operator.Type != NodeType.DotToken)
            {
                boundOperatorType = AbstractBinaryOperator.Bind(syntax.Operator.Type, boundLeft.ResultType, boundRight.ResultType);
            }
            else
            {
                boundOperatorType = AbstractBinaryOperator.Bind(boundLeft.ResultType, boundRight.ResultType);
            }

            if (boundOperatorType is null)
            {
                _binderReport.ReportUndefinedBinaryOperator(syntax.Operator.Location, syntax.Operator.Text, boundLeft.ResultType, boundRight.ResultType);
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractBinaryExpression(syntax, boundLeft, boundOperatorType, boundRight);
        }

        private AbstractExpression BindNameExpression(NameExpression syntax, AbstractExpression? parentExpression)
        {
            if (syntax.Identifier.IsMissing)
            {
                // This means the token was inserted by the parser and already reported the error
                return new AbstractErrorExpression(syntax);
            }

            if (parentExpression != null &&
                parentExpression.ResultType.IsObject)
            {
                BundleSymbol? bundleTemplate = GetBundleSymbol(parentExpression.ResultType);
                if (bundleTemplate is null)
                {
                    _binderReport.ReportBundleDoesNotExist(parentExpression.ResultType.Name, syntax.Location);
                    return new AbstractErrorExpression(syntax);
                }


                AbstractBundleField? fieldToAccess = bundleTemplate
                    .Fields.FirstOrDefault(f => f.FieldName == syntax.Identifier.Text);

                if (fieldToAccess is null)
                {
                    _binderReport.ReportFieldDoesNotExist(syntax.Identifier);
                    return new AbstractErrorExpression(syntax);
                }

                return new AbstractObjectFieldAccessExpression(syntax, fieldToAccess);
            }

            VariableSymbol? variable = BindVariableReference(syntax);
            if (variable == null)
            {
                return new AbstractErrorExpression(syntax);
            }

            return new AbstractVariableExpression(syntax, variable);
        }

        private AbstractExpression BindCallExpression(CallExpression expr)
        {
            // This is for casting of type to type, as it's treated as a function call
            if (expr.Arguments.Count == 1 && LookupType(expr.Identifier.Text) is TypeSymbol type)
            {
                return BindConversion(expr.Arguments[0], type, allowExplicit: true);
            }

            // We lookup into references too, if nothinng is found we assume it doesn't exist even if we could technically compile just fine
            FunctionSymbol? function = TryLookupFunctionSymbol(expr);
            if (function == null)
            {
                return new AbstractErrorExpression(expr);
            }

            if (expr.IsAsyncCall)
            {
                if (function.ReturnType != TypeSymbol.Void && function.ReturnType != TypeSymbol.BaseObject)
                {
                    _binderReport.ReportAsyncVariableMustBeVoidOrObject(function, expr.Location);
                    return new AbstractErrorExpression(expr);
                }
            }

            if (!AreFunctionCallArgumentOk(expr, function))
            {
                return new AbstractErrorExpression(expr);
            }

            BindFunctionCallArguments(expr, out ImmutableArray<AbstractExpression> boundArguments, function);
            return new AbstractCallExpression(expr, expr.IsAsyncCall, expr.Namespace?.Text, function, boundArguments);
        }

        private AbstractExpression BindObjectCallExpression(ObjectCallExpression expr)
        {
            string variableName = expr.ObjectIdentifier.Text;
            string functionName = expr.Identifier.Text;

            ImmutableArray<AbstractExpression>.Builder? boundArguments = ImmutableArray.CreateBuilder<AbstractExpression>();
            foreach (Expression? argument in expr.Arguments)
            {
                AbstractExpression? boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            // Get the instantiated bundle which is in a variable
            var objIdentifier = expr.ObjectIdentifier;
            VariableSymbol? localVariable = BindVariableReference(string.Empty, expr.ObjectIdentifier.Text, expr.ObjectIdentifier.Location);

            if (localVariable is null)
            {
                return new AbstractErrorExpression(expr);
            }

            FunctionSymbol? objectFunction = localVariable.Type.RegisteredFunctions.FirstOrDefault(f => f.Name == functionName);
            if (objectFunction is null)
            {
                _binderReport.ReportObjectFunctionDoesNotExist(localVariable.Type.ToString(), expr.Identifier.Location, expr.Identifier.Text);
                return new AbstractErrorExpression(expr);
            }

            if (!AreFunctionCallArgumentOk(expr, objectFunction))
            {
                return new AbstractErrorExpression(expr);
            }

            return new AbstractObjectCallExpression(expr, localVariable, objectFunction, boundArguments.ToImmutableArray());
        }

        private AbstractObjectInitializationExpression BindObjectInitializationExpression(ObjectInitializationExpression expr)
        {
            var abstractAssignmentExpressions = ImmutableArray.CreateBuilder<AbstractObjectFieldInitializationExpression>();
            foreach (var e in expr.FieldExpressions)
            {
                var abstractAssignment = BindObjectFieldInitializationExpression(e);
                abstractAssignmentExpressions.Add(abstractAssignment);
            }

            return new AbstractObjectInitializationExpression(expr, abstractAssignmentExpressions.ToImmutableArray());
        }

        private AbstractObjectFieldInitializationExpression BindObjectFieldInitializationExpression(ObjectFieldInitializationExpression expression)
        {
            var expr = BindExpression(expression.Initializer);
            return new AbstractObjectFieldInitializationExpression(expression, expression.Identifier.Text, expr);
        }

        private static AbstractArrayInitializationExpression BindArrayInitializationExpression(ArrayInitializationExpression expr)
        {
            return new AbstractArrayInitializationExpression(expr);
        }

        private AbstractIsDefinedExpression BindIsDefinedExpression(IsDefinedExpression expr)
        {
            var evalExpression = BindExpression(expr.Expression, false);
            return new AbstractIsDefinedExpression(expr, evalExpression);
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
                    {
                        firstExceedingNode = expression.Arguments.GetSeparator(function.Parameters.Length - 1);
                    }
                    else
                    {
                        firstExceedingNode = expression.Arguments[0];
                    }

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
                                            es.Expression.Type == AbstractNodeType.ObjectFieldAssignmentExpression ||
                                            es.Expression.Type == AbstractNodeType.ObjectCallExpression ||
                                            es.Expression.Type == AbstractNodeType.ArrayAssignmentExpression ||
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
            NodeType.WaitNotificationStatement => BindWaitNotificationStatement((WaitNotificationStatement)syntax),
            NodeType.EndOnNotificationKeyword => BindEndOnNotificationStatement((EndOnNotificationStatement)syntax),
            NodeType.NotifyStatement => BindNotifyStatement((NotifyStatement)syntax),
            NodeType.IfStatement => BindIfStatement((IfStatement)syntax),
            NodeType.WhileStatement => BindWhileStatement((WhileStatement)syntax),
            NodeType.DoWhileStatement => BindDoWhileStatement((DoWhileStatement)syntax),
            NodeType.ForStatement => BindForStatement((ForStatement)syntax),
            NodeType.BreakStatement => BindBreakStatement((BreakStatement)syntax),
            NodeType.ContinueStatement => BindContinueStatement((ContinueStatement)syntax),
            NodeType.ReturnStatement => BindReturnStatement((ReturnStatement)syntax),
            NodeType.VariableDeclarationCollection => BindVariableDeclarations((VariableDeclarationCollection)syntax, string.Empty),
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
                {
                    _binderReport.ReportInvalidReturnExpression(syntax.Expression!.Location, currentFunction.Name);
                }
            }
            else
            {
                // A return statement always needs an expression if return type is not void
                if (expression is null)
                {
                    _binderReport.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, currentFunction.Name, currentFunction.ReturnType);
                }
                else
                {
                    expression = BindConversion(syntax.Expression!.Location, expression, currentFunction.ReturnType);
                }
            }

            return new AbstractReturnStatement(syntax, expression);
        }

        private AbstractBlockStatement BindBlockStatement(BlockStatement node, bool createNewScope = true)
        {
            ImmutableArray<AbstractStatement>.Builder statements = ImmutableArray.CreateBuilder<AbstractStatement>();

            if(createNewScope)
            {
                //Block of codes have a new scope
                _currentScope = new(_currentScope);
            }

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
            if (timeExpr is not AbstractErrorExpression &&
                timeExpr.ResultType != TypeSymbol.Int &&
                timeExpr.ResultType != TypeSymbol.Float)
            {
                _binderReport.ReportWaitMustBeNumber(timeExpr);
                timeExpr = new AbstractErrorExpression(timeExpr.OriginalNode);
            }

            return new AbstractWaitStatement(expr, timeExpr);
        }

        private AbstractNotifyStatement BindNotifyStatement(NotifyStatement syntax)
        {
            AbstractExpression nameExpression = BindNameExpression(syntax.Identifier, null);
            if (nameExpression is AbstractVariableExpression ave && !ave.Variable.Type.IsObject)
            {
                _binderReport.ReportIdentifierNotOfType(syntax.Identifier.Location, ave.Variable.Name, TypeSymbol.BaseObject);
            }

            AbstractExpression notifyExpr = BindExpression(syntax.Expression, canBeVoid: false);
            if (notifyExpr is not AbstractErrorExpression && notifyExpr.ResultType != TypeSymbol.String)
            {
                _binderReport.ReportCannotConvertType(notifyExpr.OriginalNode.Location, notifyExpr.ResultType, TypeSymbol.String);
                notifyExpr = new AbstractErrorExpression(notifyExpr.OriginalNode);
            }

            return new AbstractNotifyStatement(syntax, nameExpression, notifyExpr);
        }

        private AbstractWaitNotificationStatement BindWaitNotificationStatement(WaitNotificationStatement syntax)
        {
            AbstractExpression nameExpression = BindNameExpression(syntax.Identifier, null);
            if (nameExpression is AbstractVariableExpression ave && !ave.Variable.Type.IsObject)
            {
                _binderReport.ReportIdentifierNotOfType(syntax.Identifier.Location, ave.Variable.Name, TypeSymbol.BaseObject);
            }

            AbstractExpression notifyExpr = BindExpression(syntax.Expression, canBeVoid: false);
            if (notifyExpr is not AbstractErrorExpression && notifyExpr.ResultType != TypeSymbol.String)
            {
                _binderReport.ReportCannotConvertType(notifyExpr.OriginalNode.Location, notifyExpr.ResultType, TypeSymbol.String);
                notifyExpr = new AbstractErrorExpression(notifyExpr.OriginalNode);
            }

            return new AbstractWaitNotificationStatement(syntax, nameExpression, notifyExpr);
        }

        private AbstractEndOnNotificationStatement BindEndOnNotificationStatement(EndOnNotificationStatement syntax)
        {
            AbstractExpression nameExpression = BindNameExpression(syntax.Identifier, null);
            if (nameExpression is AbstractVariableExpression ave && !ave.Variable.Type.IsObject)
            {
                _binderReport.ReportIdentifierNotOfType(syntax.Identifier.Location, ave.Variable.Name, TypeSymbol.BaseObject);
            }

            AbstractExpression notifyExpr = BindExpression(syntax.Expression, canBeVoid: false);
            if (notifyExpr is not AbstractErrorExpression && notifyExpr.ResultType != TypeSymbol.String)
            {
                _binderReport.ReportCannotConvertType(notifyExpr.OriginalNode.Location, notifyExpr.ResultType, TypeSymbol.String);
                notifyExpr = new AbstractErrorExpression(notifyExpr.OriginalNode);
            }

            return new AbstractEndOnNotificationStatement(syntax, nameExpression, notifyExpr);
        }

        private AbstractIfStatement BindIfStatement(IfStatement syntax)
        {
            AbstractExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if ((bool)condition.ConstantValue.Value == false)
                {
                    _binderReport.ReportUnreachableCode(syntax.ThenStatement);
                }
                else if (syntax.ElseClause != null)
                {
                    _binderReport.ReportUnreachableCode(syntax.ElseClause.ElseStatement);
                }
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
            {
                return null;
            }

            TypeSymbol? type = null;
            string typeName = typeClause.Identifier.Text;
            if (typeClause.Namespace != null && typeClause.Namespace.Text != _currentProgram.Namespace.Text)
            {
                if (_currentProgram.References.TryGetBundle(typeClause.Namespace.Text, typeName, out BundleSymbol? _))
                {
                    return new ObjectTypeSymbol(typeClause.Namespace.Text, typeName);
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
                    ObjectTypeSymbol objSymbol = new(_currentProgram.Namespace.Text, typeName);
                    if (typeClause.RankSpecifier != null && typeClause.RankSpecifier.Rank > 0)
                    {
                        if (typeClause.RankSpecifier.Rank != 1)
                        {
                            throw new NotImplementedException("Multi dimensional array are not supported");
                        }

                        return new ArrayTypeSymbol(objSymbol, typeClause.RankSpecifier.Rank);
                    }

                    return objSymbol;
                }

                _binderReport.ReportUndefinedType(typeClause.Identifier);
                return TypeSymbol.Error;
            }

            if (type is not null && typeClause.RankSpecifier != null && typeClause.RankSpecifier.Rank > 0)
            {
                if (typeClause.RankSpecifier.Rank != 1)
                {
                    throw new NotImplementedException("Multi dimensional array are not supported");
                }

                // This is an array type
                return new ArrayTypeSymbol(type, typeClause.RankSpecifier.Rank);
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
            _ => null,
        };
    }
}
