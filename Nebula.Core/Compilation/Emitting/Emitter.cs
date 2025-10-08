using Nebula.CodeGeneration;
using Nebula.CodeGeneration.Definitions;
using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Expression.Bundles;
using Nebula.Core.Compilation.AST.Tree.Operators;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow;
using Nebula.Core.Compilation.CST.Tree.Statements;
using Nebula.Interop.Enumerators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nebula.Core.Compilation.Emitting
{
    public sealed class Emitter
    {
        public class Options
        {
            public string OutputFolder { get; set; } = "";

            public bool OutputToSourceLocation { get; set; } = false;
            public bool ReadableBytecode { get; set; } = true;
        }

        /// <summary>Context used by the emitter to keep track of all the code being emitted</summary>
        private sealed class Context(string moduleName, string @namespace, CodeGeneration.Version moduleVersion, SourceCode sourceCode)
        {
            public Assembly Assembly { get; } = new(moduleName, @namespace, moduleVersion, sourceCode);
            public Report Report { get; } = new();
            public Dictionary<VariableSymbol, VariableDefinition> Locals { get; } = [];
            public Dictionary<VariableSymbol, ParameterDefinition> Parameters { get; } = [];
            public Dictionary<AbstractLabel, int> Labels { get; } = [];
            public List<(int InstructionIndex, AbstractLabel Target)> LabelReferences { get; } = [];
        }

        public string ModuleName { get; }

        private readonly Options _options;
        private Context _currentContext = null!;

        public Emitter(string moduleName, Options options)
        {
            ModuleName = moduleName;
            _options = options;
        }

        public void Emit(AbstractProgram program, out Report emitReport)
        {
            NamespaceStatement nsToken = (NamespaceStatement)program.Namespace.OriginalNode;
            _currentContext = new(ModuleName, program.Namespace.Text, new(1, 0, 0), program.SourceCode);

            foreach (FunctionSymbol nativeFunc in program.NativeFunctions)
            {
                EmitNativeFunctionDeclaration(nativeFunc);
            }

            foreach (KeyValuePair<string, BundleSymbol> bundle in program.Bundles)
            {
                EmitBundleDeclaration(bundle.Value);
            }

            foreach ((FunctionSymbol declaration, AbstractBlockStatement body) in program.Functions)
            {
                EmitFunctionDeclaration(declaration, body);
            }

            string moduleName = Path.ChangeExtension(ModuleName, ".neb");
            string moduleDbgName = Path.ChangeExtension(ModuleName, ".ndbg");

            string outputFilePath = Path.Combine(_options.OutputFolder, moduleName);
            string outputDbgFilePath = Path.Combine(_options.OutputFolder, moduleDbgName);
            if (!_options.OutputToSourceLocation)
            {
                outputFilePath = Path.Combine(_options.OutputFolder, moduleName);
                outputDbgFilePath = Path.Combine(_options.OutputFolder, moduleDbgName);
            }
            else
            {
                string sourceLoc = Path.GetDirectoryName(program.SourceCode.FileName)
                    ?? throw new Exception("Could not get source path!");

                outputFilePath = Path.Combine(sourceLoc, moduleName);
                outputDbgFilePath = Path.Combine(sourceLoc, moduleDbgName);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? throw new Exception("Path does not exist"));

            if (_options.ReadableBytecode)
            {
                using (FileStream output = File.Create(outputFilePath))
                using (FileStream outputDbg = File.Create(outputDbgFilePath))
                using (StreamWriter writer = new(output))
                using (StreamWriter dbgWriter = new(outputDbg))
                {
                    _currentContext.Assembly.Write(writer);
                    output.Flush();
                    output.Position = 0;
                    string md5Checksum = string.Empty;
                    using (MD5 md5 = MD5.Create())
                    {

                        md5Checksum = BitConverter.ToString(md5.ComputeHash(output)).Replace("-", "").ToLowerInvariant();
                    }

                    Debug.Assert(!string.IsNullOrEmpty(md5Checksum));
                    _currentContext.Assembly.WriteDebuggingInfo(dbgWriter, md5Checksum);
                }
            }
            else
            {
                throw new NotImplementedException();
                // using (FileStream output = File.Create(outputFilePath))
                // using (BinaryWriter bw = new(output))
                // {
                //     _currentContext.Assembly.Write(bw);
                // }
            }



            emitReport = _currentContext.Report;
            _currentContext = null!;
        }


        #region Emit Function
        private void EmitNativeFunctionDeclaration(FunctionSymbol nativeFunc)
        {
            _currentContext.Assembly.TypeDefinition.NativeMethods.Add(new(nativeFunc.Name));
        }

        private void EmitFunctionDeclaration(FunctionSymbol declaration, AbstractBlockStatement body)
        {
            _currentContext.Locals.Clear();
            _currentContext.Parameters.Clear();
            _currentContext.LabelReferences.Clear();
            _currentContext.Labels.Clear();

            TypeReference returnType = _knownTypes[declaration.ReturnType.BaseType];
            AttributeType attributes = GenerateAttributeMask(declaration.Attributes);
            MethodDefinition method = new(declaration.Name, attributes, returnType, declaration.Declaration);
            _currentContext.Assembly.TypeDefinition.Methods.Add(method);

            foreach (ParameterSymbol? parameter in declaration.Parameters)
            {
                TypeReference? parameterType = _knownTypes[parameter.Type.BaseType];
                ParameterDefinition parameterDefinition = new(parameterType, parameter.Name, method.Parameters.Count);
                if (parameter.Type is ObjectTypeSymbol objSymbol)
                {
                    parameterDefinition.SourceNamespace = objSymbol.Namespace;
                    parameterDefinition.SourceTypeName = objSymbol.Name;
                }

                method.Parameters.Add(parameterDefinition);
                _currentContext.Parameters.Add(parameter, parameterDefinition);
            }

            EmitFunctionBody(method, body);
        }

        private void EmitBundleDeclaration(BundleSymbol value)
        {
            string bundleName = value.Name;
            BundleDefinition bundle = new(bundleName);

            foreach (AbstractBundleField field in value.Fields)
            {
                TypeReference? fieldType = _knownTypes[field.FieldType.BaseType];
                ParameterDefinition fieldDef = new(fieldType, field.FieldName, bundle.Fields.Count);
                if (field.FieldType is ObjectTypeSymbol objSymbol)
                {
                    fieldDef.SourceNamespace = objSymbol.Namespace;
                    fieldDef.SourceTypeName = objSymbol.Name;
                }

                bundle.Fields.Add(fieldDef);
            }

            _currentContext.Assembly.TypeDefinition.Bundles.Add(bundle);
        }


        private void EmitFunctionBody(MethodDefinition method, AbstractBlockStatement body)
        {
            NILProcessor processor = method.Body.NILProcessor;

            foreach (AbstractStatement statement in body.Statements)
            {
                EmitStatement(processor, statement);
            }

            foreach ((int InstructionIndex, AbstractLabel Target) in _currentContext.LabelReferences)
            {
                AbstractLabel? targetLabel = Target;
                int targetInstructionIndex = _currentContext.Labels[targetLabel];
                //Instruction? targetInstruction = processor.Body.Instructions[targetInstructionIndex];
                Instruction? instructionToFix = processor.Body.Instructions[InstructionIndex];
                //instructionToFix.Operand = targetInstruction;
                instructionToFix.Operand = targetInstructionIndex;
            }
        }

        #endregion

        #region Statement emission
        private void EmitStatement(NILProcessor processor, AbstractStatement node)
        {
            switch (node.Type)
            {
                case AbstractNodeType.NopStatement:
                    EmitNopStatement(processor, (AbstractNopStatement)node);
                    break;
                case AbstractNodeType.ExpressionStatement:
                    EmitExpressionStatement(processor, (AbstractExpressionStatement)node);
                    break;
                case AbstractNodeType.VariableDeclarationCollection:
                    EmitVariableDeclaration(processor, (AbstractVariableDeclarationCollection)node);
                    break;
                case AbstractNodeType.ConditionalGotoStatement:
                    EmitConditionalGotoStatement(processor, (AbstractConditionalGotoStatement)node);
                    break;
                case AbstractNodeType.GotoStatement:
                    EmitGotoStatement(processor, (AbstractGotoStatement)node);
                    break;
                case AbstractNodeType.LabelStatement:
                    EmitLabelStatement(processor, (AbstractLabelStatement)node);
                    break;
                case AbstractNodeType.ReturnStatement:
                    EmitReturnStatement(processor, (AbstractReturnStatement)node);
                    break;
                case AbstractNodeType.WaitStatement:
                    EmitWaitStatement(processor, (AbstractWaitStatement)node);
                    break;
                case AbstractNodeType.WaitNotificationStatement:
                    EmitWaitNotificationStatement(processor, (AbstractWaitNotificationStatement)node);
                    break;
                case AbstractNodeType.NotifyStatement:
                    EmitNotifyStatement(processor, (AbstractNotifyStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected node type '{node.Type}'");
            }
        }

        private void EmitWaitStatement(NILProcessor processor, AbstractWaitStatement node)
        {
            EmitExpression(processor, node.TimeExpression, node.OriginalNode);
            processor.Emit(InstructionOpcode.Wait, node.OriginalNode);
        }

        private void EmitWaitNotificationStatement(NILProcessor processor, AbstractWaitNotificationStatement node)
        {
            EmitExpression(processor, node.BundleToWaitOn, node.OriginalNode);
            EmitExpression(processor, node.NotifyExpression, node.OriginalNode);
            processor.Emit(InstructionOpcode.Wait_n, node.OriginalNode);
        }

        private void EmitNotifyStatement(NILProcessor processor, AbstractNotifyStatement node)
        {
            EmitExpression(processor, node.BundleToNotifyFrom, node.OriginalNode);
            EmitExpression(processor, node.NotifyExpression, node.OriginalNode);
            processor.Emit(InstructionOpcode.Notify, node.OriginalNode);
        }

        private void EmitReturnStatement(NILProcessor processor, AbstractReturnStatement node)
        {
            if (node.Expression != null)
            {
                EmitExpression(processor, node.Expression, node.OriginalNode);
            }

            processor.Emit(InstructionOpcode.Ret, node.OriginalNode);
        }

        private void EmitLabelStatement(NILProcessor processor, AbstractLabelStatement node)
        {
            _currentContext.Labels.Add(node.Label, processor.Body.Instructions.Count);
        }

        private void EmitGotoStatement(NILProcessor processor, AbstractGotoStatement node)
        {
            _currentContext.LabelReferences.Add((processor.Body.Instructions.Count, node.Label));
            processor.Emit(InstructionOpcode.Br,
                new Instruction(InstructionOpcode.Nop, node.OriginalNode),
                node.OriginalNode);
        }

        private void EmitConditionalGotoStatement(NILProcessor processor, AbstractConditionalGotoStatement node)
        {
            EmitExpression(processor, node.Condition, node.OriginalNode);

            InstructionOpcode opCode = node.JumpIfTrue ? InstructionOpcode.BrTrue : InstructionOpcode.BrFalse;
            _currentContext.LabelReferences.Add((processor.Body.Instructions.Count, node.Label));

            processor.Emit(opCode,
                new Instruction(InstructionOpcode.Nop, node.OriginalNode),
                node.OriginalNode);
        }

        private void EmitVariableDeclaration(NILProcessor processor, AbstractVariableDeclarationCollection node)
        {
            foreach (AbstractVariableDeclaration declaration in node.AllVariables)
            {
                EmitVariableDeclaration(processor, declaration, node.OriginalNode);
            }
        }

        private void EmitVariableDeclaration(NILProcessor processor, AbstractVariableDeclaration node, Node originalStatement)
        {
            TypeReference typeReference = _knownTypes[node.Variable.Type.BaseType];

            VariableDefinition variableDefinition = new(typeReference, node.Variable.Name, processor.Body.Variables.Count);

            if (node.Variable.Type is ObjectTypeSymbol objSymbol)
            {
                variableDefinition.SourceNamespace = objSymbol.Namespace;
                variableDefinition.SourceTypeName = objSymbol.Name;
            }

            _currentContext.Locals.Add(node.Variable, variableDefinition);
            processor.Body.Variables.Add(variableDefinition);

            // Emit an instruction to allocate bundle
            if (typeReference == TypeReference.Bundle && node.Variable.Type is ObjectTypeSymbol objTypeSymbol)
            {
                ExtractBundleNamespaceAndName(objTypeSymbol, out string typeNamespace, out string typedName);

                object arguments = typedName;
                if (_currentContext.Assembly.Namespace != typeNamespace)
                {
                    arguments = new string[] { typeNamespace, typedName };
                }

                processor.Emit(InstructionOpcode.Newobj, arguments, originalStatement);
                processor.Emit(InstructionOpcode.Stloc, variableDefinition, originalStatement);
                return;
            }

            if (typeReference == TypeReference.Array)
            {
                ArrayTypeSymbol arraySymbol = (ArrayTypeSymbol)node.Variable.Type;
                TypeReference arrayValueType = _knownTypes[arraySymbol.ValueType.BaseType];

                StoreNewArrIntoLocal(processor, variableDefinition, arraySymbol, arrayValueType, originalStatement);
                return;
            }

            EmitExpression(processor, node.Initializer, originalStatement);
            processor.Emit(InstructionOpcode.Stloc, variableDefinition, originalStatement);
        }

        private void StoreNewArrIntoLocal(NILProcessor processor,
                                          VariableDefinition variableDefinition,
                                          ArrayTypeSymbol arraySymbol,
                                          TypeReference baseValueType,
                                          Node originalNode)
        {
            object arguments = ExtractNewArrArguments(arraySymbol, baseValueType);

            processor.Emit(InstructionOpcode.Newarr, arguments, originalNode);
            processor.Emit(InstructionOpcode.Stloc, variableDefinition, originalNode);
        }

        private void StoreNewArrIntoParameter(NILProcessor processor,
                                          ParameterDefinition parameter,
                                          ArrayTypeSymbol arraySymbol,
                                          TypeReference baseValueType,
                                          Node originalNode)
        {
            object arguments = ExtractNewArrArguments(arraySymbol, baseValueType);

            processor.Emit(InstructionOpcode.Newarr, arguments, originalNode);
            processor.Emit(InstructionOpcode.StArg, parameter, originalNode);
        }

        private object ExtractNewArrArguments(ArrayTypeSymbol arraySymbol, TypeReference baseValueType)
        {
            object arguments = baseValueType.Name;
            if (arraySymbol.ValueType.IsObject)
            {
                ExtractBundleNamespaceAndName((ObjectTypeSymbol)arraySymbol.ValueType, out string typeNamespace, out string typedName);
                if (_currentContext.Assembly.Namespace != typeNamespace)
                {
                    arguments = new string[] { baseValueType.Name, typeNamespace, typedName };
                }
                else
                {
                    arguments = new string[] { baseValueType.Name, typedName };
                }
            }

            if (arraySymbol.ValueType.IsArray)
            {
                throw new NotSupportedException("Array within array is not supported!");
            }

            return arguments;
        }

        private static void ExtractBundleNamespaceAndName(ObjectTypeSymbol objTypeSymbol, out string typeNamespace, out string typedName)
        {
            typeNamespace = string.Empty;
            typedName = string.Empty;
            if (!string.IsNullOrEmpty(objTypeSymbol.Namespace))
            {
                typeNamespace = objTypeSymbol.Namespace;
            }

            if (!string.IsNullOrEmpty(objTypeSymbol.Name))
            {
                typedName = objTypeSymbol.Name;
            }
        }

        private void EmitExpressionStatement(NILProcessor processor, AbstractExpressionStatement node)
        {
            EmitExpression(processor, node.Expression, node.OriginalNode);
            if (node.Expression.ResultType != TypeSymbol.Void)
            {
                processor.Emit(InstructionOpcode.Pop, node.OriginalNode);
            }
        }

        private static void EmitNopStatement(NILProcessor processor, AbstractNopStatement node)
        {
            processor.Emit(InstructionOpcode.Nop, node.OriginalNode);
        }

        #endregion

        #region Expression emission

        private void EmitExpression(NILProcessor processor, AbstractExpression node, Node originalStatement)
        {
            if (node.ConstantValue != null)
            {
                EmitConstantExpression(processor, node, originalStatement);
                return;
            }

            // Literal expressions are not processed and folded into constants
            switch (node.Type)
            {
                case AbstractNodeType.ErrorExpression:
                    EmitErrorExpression(processor, (AbstractErrorExpression)node, originalStatement);
                    break;
                case AbstractNodeType.UnaryExpression:
                    EmitUnaryExpression(processor, (AbstractUnaryExpression)node, originalStatement);
                    break;
                case AbstractNodeType.BinaryExpression:
                    EmitBinaryExpression(processor, (AbstractBinaryExpression)node, originalStatement);
                    break;
                case AbstractNodeType.VariableExpression:
                    EmitVariableExpression(processor, (AbstractVariableExpression)node, originalStatement);
                    break;
                case AbstractNodeType.ObjectFieldAssignmentExpression:
                    EmitBundleFieldAssignmentExpression(processor, (AbstractObjectFieldAssignmentExpression)node, originalStatement);
                    break;
                case AbstractNodeType.ArrayAssignmentExpression:
                    EmitArrayAssignmentExpression(processor, (AbstractArrayAssignmentExpression)node, originalStatement);
                    break;
                case AbstractNodeType.AssignmentExpression:
                    EmitAssignmentExpression(processor, (AbstractAssignmentExpression)node, originalStatement);
                    break;
                case AbstractNodeType.CallExpression:
                    EmitCallExpression(processor, (AbstractCallExpression)node, originalStatement);
                    break;
                case AbstractNodeType.ConversionExpression:
                    EmitConversionExpression(processor, (AbstractConversionExpression)node, originalStatement);
                    break;
                case AbstractNodeType.ObjectCallExpression:
                    EmitObjectCallExpression(processor, (AbstractObjectCallExpression)node, originalStatement);
                    break;
                case AbstractNodeType.ArrayAccessExpression:
                    EmitArrayAccessExpression(processor, (AbstractArrayAccessExpression)node, originalStatement);
                    break;
                default:
                    throw new Exception($"Unexpected node type {node.Type}");
            }
        }

        private void EmitConversionExpression(NILProcessor processor, AbstractConversionExpression node, Node originalStatement)
        {
            EmitExpression(processor, node.Expression, originalStatement);
            processor.Emit(InstructionOpcode.ConvType, _knownTypes[node.ResultType.BaseType], originalStatement);
        }

        private void EmitObjectCallExpression(NILProcessor processor, AbstractObjectCallExpression node, Node originalStatement)
        {
            foreach (AbstractExpression argument in node.Arguments)
            {
                EmitExpression(processor, argument, originalStatement);
            }

            VariableDefinition? variableDefinition = _currentContext.Locals[node.Variable];
            processor.Emit(InstructionOpcode.Callvirt, new string[] { variableDefinition.Index.ToString(), node.Function.Name }, originalStatement);
        }

        private void EmitArrayAccessExpression(NILProcessor processor, AbstractArrayAccessExpression node, Node originalStatement)
        {
            if (node.ArrayVariable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[parameter];
                processor.Emit(InstructionOpcode.Ldarg, parameterDefinition, originalStatement);
            }
            else
            {
                VariableDefinition? variableDefinition = _currentContext.Locals[node.ArrayVariable];
                processor.Emit(InstructionOpcode.Ldloc, variableDefinition, originalStatement);
            }

            EmitExpression(processor, node.IndexExpression, originalStatement);
            processor.Emit(InstructionOpcode.Ldelem, originalStatement);
        }

        private void EmitCallExpression(NILProcessor processor, AbstractCallExpression node, Node originalStatement)
        {
            // Emit all the arguments
            foreach (AbstractExpression argument in node.Arguments)
            {
                EmitExpression(processor, argument, originalStatement);
            }

            InstructionOpcode callInstruction = node.IsAsync ? InstructionOpcode.Call_t : InstructionOpcode.Call;

            object arguments = node.Function.Name;
            if (!string.IsNullOrEmpty(node.Namespace))
            {
                arguments = new string[] { node.Namespace, node.Function.Name };
            }

            processor.Emit(callInstruction, arguments, originalStatement);
        }

        private void EmitBundleFieldAssignmentExpression(NILProcessor processor, AbstractObjectFieldAssignmentExpression node, Node originalStatement)
        {
            if (node.BundleVariable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[node.BundleVariable];
                EmitExpression(processor, node.Expression, originalStatement);
                processor.Emit(InstructionOpcode.Dup,
                    originalStatement); // Takes current value on stack and pushes it again

                processor.Emit(InstructionOpcode.StBArg,
                    new int[] { parameter.OrdinalPosition, node.FieldToAssign.OrdinalPosition },
                    originalStatement); // Writes value into local
                return;
            }

            VariableDefinition? variableDefinition = _currentContext.Locals[node.BundleVariable];
            EmitExpression(processor, node.Expression, originalStatement);
            processor.Emit(InstructionOpcode.Dup,
                originalStatement); // Takes current value on stack and pushes it again

            // TODO Add a concept of bundle fields in the emitter too
            processor.Emit(InstructionOpcode.StBloc,
                new int[] { variableDefinition.Index, node.FieldToAssign.OrdinalPosition },
                originalStatement); // Writes value into local
        }

        private void EmitArrayAssignmentExpression(NILProcessor processor, AbstractArrayAssignmentExpression node, Node originalStatement)
        {
            if (node.ArrayVariable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[parameter];
                processor.Emit(InstructionOpcode.Ldarg, parameterDefinition.Index, originalStatement);
            }
            else
            {
                VariableDefinition? variableDefinition = _currentContext.Locals[node.ArrayVariable];
                processor.Emit(InstructionOpcode.Ldloc, variableDefinition, originalStatement);
            }

            EmitExpression(processor, node.IndexExpression, originalStatement);
            EmitExpression(processor, node.Expression, originalStatement);
            processor.Emit(InstructionOpcode.Stelem, originalStatement);
        }

        private void EmitAssignmentExpression(NILProcessor processor, AbstractAssignmentExpression node, Node originalStatement)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[node.Variable];
                if (parameter.Type == TypeSymbol.BaseObject)
                {
                    throw new NotImplementedException();
                }

                if (parameter.Type == TypeSymbol.BaseArray)
                {
                    EmitArrayParameterReassignment(processor, parameterDefinition, node, originalStatement);
                    return;
                }

                EmitExpression(processor, node.Expression, originalStatement);
                processor.Emit(InstructionOpcode.Dup, originalStatement); // Takes current value on stack and pushes it again
                processor.Emit(InstructionOpcode.StArg, parameterDefinition, originalStatement); // Writes value into parameter
                return;
            }

            VariableDefinition? variableDefinition = _currentContext.Locals[node.Variable];

            if (node.Variable.Type == TypeSymbol.BaseObject)
            {
                throw new NotImplementedException();
            }

            if (node.Variable.Type == TypeSymbol.BaseArray)
            {
                EmitArrayLocalReassignment(processor, variableDefinition, node);
                return;
            }

            EmitExpression(processor, node.Expression, originalStatement);
            processor.Emit(InstructionOpcode.Dup, originalStatement); // Takes current value on stack and pushes it again
            processor.Emit(InstructionOpcode.Stloc, variableDefinition, originalStatement); // Writes value into local
        }

        private void EmitVariableExpression(NILProcessor processor, AbstractVariableExpression node, Node originalStatement)
        {
            // TODO :: Rethink how variables vs bundles are handled to simplify the instruction set
            if (node.ArrayVariable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[parameter];

                if (node is AbstractArrayAccessExpression arrAccess)
                {
                    processor.Emit(InstructionOpcode.Ldarg, parameterDefinition.Index, originalStatement);
                    EmitExpression(processor, arrAccess.IndexExpression, originalStatement);
                    processor.Emit(InstructionOpcode.Ldc_i4, arrAccess.IndexExpression, originalStatement);
                }

                InstructionOpcode paramOpcode = InstructionOpcode.Ldarg;
                object paramArg = parameterDefinition.Index;
                if (node is AbstractBundleFieldAccessExpression fParam)
                {
                    paramOpcode = InstructionOpcode.LdBarg;
                    paramArg = new int[] { parameterDefinition.Index, fParam.Field.OrdinalPosition };
                }

                processor.Emit(paramOpcode, paramArg, originalStatement);
                return;
            }

            // TODO :: Figure out if we want to keep this variable definition as instruction argument or pass the index
            // directly

            VariableDefinition? variableDefinition = _currentContext.Locals[node.ArrayVariable];
            InstructionOpcode opcode = InstructionOpcode.Ldloc;
            object argument = variableDefinition;
            if (node is AbstractBundleFieldAccessExpression f)
            {
                opcode = InstructionOpcode.LdBloc;
                argument = new int[] { variableDefinition.Index, f.Field.OrdinalPosition };
            }

            processor.Emit(opcode, argument, originalStatement);
        }

        private void EmitBinaryExpression(NILProcessor processor, AbstractBinaryExpression node, Node originalStatement)
        {
            // +(string, string)
            if (node.Operator.BinaryType == AbstractBinaryType.Addition)
            {
                if (node.Left.ResultType == TypeSymbol.String && node.Right.ResultType == TypeSymbol.String)
                {
                    //ilProcessor.Emit(OpCodes.Call, StringConcat2Reference);
                    EmitStringConcatExpression(processor, node, originalStatement);
                    return;
                }
            }

            EmitExpression(processor, node.Left, originalStatement);
            EmitExpression(processor, node.Right, originalStatement);

            switch (node.Operator.BinaryType)
            {
                case AbstractBinaryType.Addition:
                    processor.Emit(InstructionOpcode.Add, originalStatement);
                    break;
                case AbstractBinaryType.Subtraction:
                    processor.Emit(InstructionOpcode.Sub, originalStatement);
                    break;
                case AbstractBinaryType.Multiplication:
                    processor.Emit(InstructionOpcode.Mul, originalStatement);
                    break;
                case AbstractBinaryType.Division:
                    processor.Emit(InstructionOpcode.Div, originalStatement);
                    break;
                case AbstractBinaryType.Remainer:
                    processor.Emit(InstructionOpcode.Rem, originalStatement);
                    break;
                case AbstractBinaryType.LogicalAnd:
                case AbstractBinaryType.BitwiseAnd:
                    processor.Emit(InstructionOpcode.And, originalStatement);
                    break;
                case AbstractBinaryType.BitwiseOr:
                case AbstractBinaryType.LogicalOr:
                    processor.Emit(InstructionOpcode.Or, originalStatement);
                    break;
                case AbstractBinaryType.BitwiseXor:
                    processor.Emit(InstructionOpcode.Xor, originalStatement);
                    break;
                case AbstractBinaryType.Equals:
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    break;
                case AbstractBinaryType.NotEquals:
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, originalStatement);
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    break;
                case AbstractBinaryType.LessThan:
                    processor.Emit(InstructionOpcode.Clt, originalStatement);
                    break;
                case AbstractBinaryType.LessThanOrEqual:
                    processor.Emit(InstructionOpcode.Cgt, originalStatement);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, originalStatement);
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    break;
                case AbstractBinaryType.GreaterThan:
                    processor.Emit(InstructionOpcode.Cgt, originalStatement);
                    break;
                case AbstractBinaryType.GreaterThanOrEqual:
                    processor.Emit(InstructionOpcode.Clt, originalStatement);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, originalStatement);
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    break;
                default:
                    throw new Exception($"Unexpected binary operator {SyntaxEx.GetText(node.Operator.NodeType)}");
            }
        }

        private void EmitStringConcatExpression(NILProcessor processor, AbstractBinaryExpression node, Node originalStatement)
        {
            // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
            // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
            // Example: folding b and c in ((a + b) + c) if they are constant.

            List<AbstractExpression>? nodes = FoldConstants(originalStatement, Flatten(node)).ToList();

            switch (nodes.Count)
            {
                case 0:
                    processor.Emit(InstructionOpcode.Ldc_s, string.Empty, originalStatement);
                    break;

                //case 1:
                //    EmitExpression(ilProcessor, nodes[0]);
                //    break;
                //
                //case 2:
                //    EmitExpression(ilProcessor, nodes[0]);
                //    EmitExpression(ilProcessor, nodes[1]);
                //    ilProcessor.Emit(OpCodes.Call, StringConcat2Reference);
                //    break;
                //
                //case 3:
                //    EmitExpression(ilProcessor, nodes[0]);
                //    EmitExpression(ilProcessor, nodes[1]);
                //    EmitExpression(ilProcessor, nodes[2]);
                //    ilProcessor.Emit(OpCodes.Call, StringConcat3Reference);
                //    break;
                //
                //case 4:
                //    EmitExpression(ilProcessor, nodes[0]);
                //    EmitExpression(ilProcessor, nodes[1]);
                //    EmitExpression(ilProcessor, nodes[2]);
                //    EmitExpression(ilProcessor, nodes[3]);
                //    ilProcessor.Emit(OpCodes.Call, StringConcat4Reference);
                //    break;

                default:
                    // Emit all string expressions by the end all strings should be on stack to be concat
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        EmitExpression(processor, nodes[i], originalStatement);
                    }

                    processor.Emit(InstructionOpcode.AddStr, nodes.Count, originalStatement);
                    break;
            }

            // (a + b) + (c + d) --> [a, b, c, d]
            static IEnumerable<AbstractExpression> Flatten(AbstractExpression node)
            {
                if (node is AbstractBinaryExpression binaryExpression &&
                    binaryExpression.Operator.BinaryType == AbstractBinaryType.Addition &&
                    binaryExpression.Left.ResultType == TypeSymbol.String &&
                    binaryExpression.Right.ResultType == TypeSymbol.String)
                {
                    foreach (AbstractExpression result in Flatten(binaryExpression.Left))
                    {
                        yield return result;
                    }

                    foreach (AbstractExpression result in Flatten(binaryExpression.Right))
                    {
                        yield return result;
                    }
                }
                else
                {
                    if (node.ResultType != TypeSymbol.String)
                    {
                        throw new Exception($"Unexpected node type in string concatenation: {node.Type}");
                    }

                    yield return node;
                }
            }

            // [a, "foo", "bar", b, ""] --> [a, "foobar", b]
            static IEnumerable<AbstractExpression> FoldConstants(Node syntax, IEnumerable<AbstractExpression> nodes)
            {
                StringBuilder? sb = null;

                foreach (AbstractExpression? node in nodes)
                {
                    if (node.ConstantValue != null)
                    {
                        string? stringValue = (string)node.ConstantValue.Value;

                        if (string.IsNullOrEmpty(stringValue))
                        {
                            continue;
                        }

                        sb ??= new StringBuilder();
                        sb.Append(stringValue);
                    }
                    else
                    {
                        if (sb?.Length > 0)
                        {
                            yield return new AbstractLiteralExpression(syntax, sb.ToString());
                            sb.Clear();
                        }

                        yield return node;
                    }
                }

                if (sb?.Length > 0)
                {
                    yield return new AbstractLiteralExpression(syntax, sb.ToString());
                }
            }

        }

        private void EmitUnaryExpression(NILProcessor processor, AbstractUnaryExpression node, Node originalStatement)
        {
            EmitExpression(processor, node.Operand, originalStatement);
            switch (node.Operator.UnaryType)
            {
                case AbstractUnaryType.Identity:
                    // NOP
                    break;
                case AbstractUnaryType.LogicalNegation:
                    processor.Emit(InstructionOpcode.Ldc_i4_0, originalStatement);
                    processor.Emit(InstructionOpcode.Ceq, originalStatement);
                    // !
                    break;
                case AbstractUnaryType.Negation:
                    processor.Emit(InstructionOpcode.Neg, originalStatement);
                    // -
                    break;
                case AbstractUnaryType.OnesComplement:
                    processor.Emit(InstructionOpcode.Not, originalStatement);
                    // ~
                    break;
                default:
                    throw new Exception($"Unexpected unary operator: ({SyntaxEx.GetText(node.Operator.NodeType)})({node.Operand.Type})");
            }
        }
        private static void EmitErrorExpression(NILProcessor processor, AbstractErrorExpression node, Node originalStatement)
            => throw new NotImplementedException();

        private static void EmitConstantExpression(NILProcessor processor, AbstractExpression node, Node originalStatement)
        {
            // A constant cannot be null here
            Debug.Assert(node.ConstantValue is not null);
            if (node.ResultType == TypeSymbol.Int)
            {
                int value = (int)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_i4, value, originalStatement);
                return;
            }

            if (node.ResultType == TypeSymbol.Float)
            {
                float value = (float)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_r4, value, originalStatement);
                return;
            }

            if (node.ResultType == TypeSymbol.Bool)
            {
                bool value = (bool)node.ConstantValue.Value;
                InstructionOpcode instruction = value ? InstructionOpcode.Ldc_i4_1 : InstructionOpcode.Ldc_i4_0;
                processor.Emit(instruction, originalStatement);
                return;
            }

            if (node.ResultType == TypeSymbol.String)
            {
                string? value = (string)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_s, value, originalStatement);
                return;
            }

            throw new Exception($"Unexpected constant expression type: {node.Type}");
        }

        #endregion

        private static AttributeType GenerateAttributeMask(ImmutableArray<AttributeSymbol> attributes)
        {
            AttributeType mask = AttributeType.Unknown;
            foreach (AttributeSymbol attr in attributes)
            {
                if (!attr.IsMethodAttribute || attr.Attribute is null)
                {
                    throw new InvalidDataException(nameof(attr.Attribute));
                }

                mask |= (AttributeType)attr.Attribute;
            }

            return mask;
        }

        private readonly Dictionary<TypeSymbol, TypeReference> _knownTypes = new()
        {
            {TypeSymbol.Error, TypeReference.Unknown},
            {TypeSymbol.Void, TypeReference.Void},
            {TypeSymbol.Bool, TypeReference.Bool},
            {TypeSymbol.Int, TypeReference.Int},
            {TypeSymbol.Float, TypeReference.Float},
            {TypeSymbol.String, TypeReference.String},
            {TypeSymbol.BaseObject, TypeReference.Bundle},
            {TypeSymbol.BaseArray, TypeReference.Array},
        };

        private void EmitArrayParameterReassignment(NILProcessor processor, ParameterDefinition parameter, AbstractAssignmentExpression node, Node originalStatement)
        {
            ArrayTypeSymbol arraySymbol = (ArrayTypeSymbol)node.Variable.Type;
            TypeReference typeReference = _knownTypes[arraySymbol.ValueType.BaseType];
            StoreNewArrIntoParameter(processor, parameter, arraySymbol, typeReference, originalStatement);
        }

        private void EmitArrayLocalReassignment(NILProcessor processor, VariableDefinition variableDefinition, AbstractAssignmentExpression node)
        {
            ArrayTypeSymbol arraySymbol = (ArrayTypeSymbol)node.Variable.Type;
            TypeReference typeReference = _knownTypes[arraySymbol.ValueType.BaseType];
            StoreNewArrIntoLocal(processor, variableDefinition, arraySymbol, typeReference, node.OriginalNode);
        }
    }
}
