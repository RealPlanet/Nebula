using Nebula.CodeEmitter;
using Nebula.CodeEmitter.Types;
using Nebula.Commons.Reporting;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Binding;
using Nebula.Core.Binding.Symbols;
using Nebula.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nebula.Core.Emitting
{
    public sealed class Emitter
    {
        /// <summary>Context used by the emitter to keep track of all the code being emitted</summary>
        private sealed class Context(string moduleName, string @namespace, CodeEmitter.Version moduleVersion, SourceCode sourceCode)
        {
            public Assembly Assembly { get; } = new(moduleName, @namespace, moduleVersion, sourceCode);
            public Report Report { get; } = new();
            public Dictionary<VariableSymbol, VariableDefinition> Locals { get; } = [];
            public Dictionary<VariableSymbol, ParameterDefinition> Parameters { get; } = [];
            public Dictionary<AbstractLabel, int> Labels { get; } = [];
            public List<(int InstructionIndex, AbstractLabel Target)> LabelReferences { get; } = [];
        }

        public string ModuleName { get; }
        public string OutputFolder { get; }

        private Context _currentContext = null!;

        public Emitter(string moduleName, string outputFolder)
        {
            ModuleName = moduleName;
            OutputFolder = outputFolder;
        }

        public void Emit(AbstractProgram program, bool readableBytecode, out Report emitReport)
        {
            NamespaceStatement nsToken = (NamespaceStatement)program.Namespace.OriginalNode;
            _currentContext = new(ModuleName, program.Namespace.Text, new(1, 0, 0), program.SourceCode);

            foreach(var nativeFunc in program.NativeFunctions)
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
            string outputFilePath = Path.Combine(OutputFolder, moduleName);
            string outputDbgFilePath = Path.Combine(OutputFolder, moduleDbgName);

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? throw new Exception("Path does not exist"));

            if (readableBytecode)
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

            TypeReference returnType = _knownTypes[declaration.ReturnType];
            NativeAttribute attributes = GenerateAttributeMask(declaration.Attributes);
            MethodDefinition method = new(declaration.Name, attributes, returnType, declaration.Declaration?.Span ?? TextSpan.Empty);
            _currentContext.Assembly.TypeDefinition.Methods.Add(method);

            foreach (ParameterSymbol? parameter in declaration.Parameters)
            {
                TypeReference? parameterType = _knownTypes[parameter.Type];
                ParameterDefinition parameterDefinition = new(parameterType, parameter.Name, method.Parameters.Count);
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
                TypeReference? fieldType = _knownTypes[field.FieldType];
                ParameterDefinition fieldDef = new(fieldType, field.FieldName, bundle.Fields.Count);
                bundle.Fields.Add(fieldDef);
            }

            _currentContext.Assembly.TypeDefinition.Bundles.Add(bundle);
        }


        private void EmitFunctionBody(MethodDefinition method, AbstractBlockStatement body)
        {
            NILProcessor processor = method.Body.NILProcessor;

            foreach (AbstractStatement statement in body.Statements)
                EmitStatement(processor, statement);

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
            EmitExpression(processor, node.TimeExpression);
            processor.Emit(InstructionOpcode.Wait, node.OriginalNode.Span);
        }

        private void EmitWaitNotificationStatement(NILProcessor processor, AbstractWaitNotificationStatement node)
        {
            EmitExpression(processor, node.BundleToWaitOn);
            EmitExpression(processor, node.NotifyExpression);
            processor.Emit(InstructionOpcode.Wait_n, node.OriginalNode.Span);
        }

        private void EmitNotifyStatement(NILProcessor processor, AbstractNotifyStatement node)
        {
            EmitExpression(processor, node.BundleToNotifyFrom);
            EmitExpression(processor, node.NotifyExpression);
            processor.Emit(InstructionOpcode.Notify, node.OriginalNode.Span);
        }

        private void EmitReturnStatement(NILProcessor processor, AbstractReturnStatement node)
        {
            if (node.Expression != null)
                EmitExpression(processor, node.Expression);
            processor.Emit(InstructionOpcode.Ret, node.OriginalNode.Span);
        }

        private void EmitLabelStatement(NILProcessor processor, AbstractLabelStatement node)
        {
            _currentContext.Labels.Add(node.Label, processor.Body.Instructions.Count);
        }

        private void EmitGotoStatement(NILProcessor processor, AbstractGotoStatement node)
        {
            _currentContext.LabelReferences.Add((processor.Body.Instructions.Count, node.Label));
            TextSpan location = node.OriginalNode.Span;
            processor.Emit(InstructionOpcode.Br,
                new Instruction(InstructionOpcode.Nop, location),
                location);
        }

        private void EmitConditionalGotoStatement(NILProcessor processor, AbstractConditionalGotoStatement node)
        {
            EmitExpression(processor, node.Condition);

            InstructionOpcode opCode = node.JumpIfTrue ? InstructionOpcode.Brtrue : InstructionOpcode.Brfalse;
            _currentContext.LabelReferences.Add((processor.Body.Instructions.Count, node.Label));

            TextSpan location = node.OriginalNode.Span;
            processor.Emit(opCode,
                new Instruction(InstructionOpcode.Nop, location),
                location);
        }

        private void EmitVariableDeclaration(NILProcessor processor, AbstractVariableDeclarationCollection node)
        {
            foreach (AbstractVariableDeclaration declaration in node.AllVariables)
            {
                EmitVariableDeclaration(processor, declaration);
            }
        }

        private void EmitVariableDeclaration(NILProcessor processor, AbstractVariableDeclaration node)
        {
            TypeReference typeReference = _knownTypes[node.Variable.Type];

            VariableDefinition variableDefinition = new(typeReference, node.Variable.Name, processor.Body.Variables.Count);
            _currentContext.Locals.Add(node.Variable, variableDefinition);
            processor.Body.Variables.Add(variableDefinition);
            // Emit an instruction to allocate bundle
            if (typeReference == TypeReference.Bundle && node.Initializer is AbstractConversionExpression)
            {
                string typeNamespace = string.Empty;
                string typedName = string.Empty;
                if (!string.IsNullOrEmpty(node.Variable.Type.Namespace))
                    typeNamespace = node.Variable.Type.Namespace;

                if (!string.IsNullOrEmpty(node.Variable.Type.Alias))
                    typedName = node.Variable.Type.Alias;

                object arguments = typedName;
                if (_currentContext.Assembly.Namespace != typeNamespace)
                {
                    arguments = new string[] { typeNamespace, typedName };
                }
                processor.Emit(InstructionOpcode.Ld_b, arguments, node.OriginalNode.Span);
                processor.Emit(InstructionOpcode.Stloc, variableDefinition, node.OriginalNode.Span);
                return;
            }

            EmitExpression(processor, node.Initializer);
            processor.Emit(InstructionOpcode.Stloc, variableDefinition, node.OriginalNode.Span);
        }

        private void EmitExpressionStatement(NILProcessor processor, AbstractExpressionStatement node)
        {
            EmitExpression(processor, node.Expression);
            if (node.Expression.ResultType != TypeSymbol.Void)
                processor.Emit(InstructionOpcode.Pop, node.OriginalNode.Span);
        }

        private static void EmitNopStatement(NILProcessor processor, AbstractNopStatement node)
        {
            processor.Emit(InstructionOpcode.Nop, node.OriginalNode.Span);
        }

        #endregion

        #region Expression emission

        private void EmitExpression(NILProcessor processor, AbstractExpression node)
        {
            if (node.ConstantValue != null)
            {
                EmitConstantExpression(processor, node);
                return;
            }

            // Literal expressions are not processed and folded into constants
            switch (node.Type)
            {
                case AbstractNodeType.ErrorExpression:
                    EmitErrorExpression(processor, (AbstractErrorExpression)node);
                    break;
                case AbstractNodeType.UnaryExpression:
                    EmitUnaryExpression(processor, (AbstractUnaryExpression)node);
                    break;
                case AbstractNodeType.BinaryExpression:
                    EmitBinaryExpression(processor, (AbstractBinaryExpression)node);
                    break;
                case AbstractNodeType.VariableExpression:
                    EmitVariableExpression(processor, (AbstractVariableExpression)node);
                    break;
                case AbstractNodeType.BundleFieldAssignmentExpression:
                    EmitBundleFieldAssignmentExpression(processor, (AbstractBundleFieldAssignment)node);
                    break;
                case AbstractNodeType.AssignmentExpression:
                    EmitAssignmentExpression(processor, (AbstractAssignmentExpression)node);
                    break;
                case AbstractNodeType.CallExpression:
                    EmitCallExpression(processor, (AbstractCallExpression)node);
                    break;
                case AbstractNodeType.ConversionExpression:
                    EmitConversionExpression(processor, (AbstractConversionExpression)node);
                    break;
                default:
                    throw new Exception($"Unexpected node type {node.Type}");
            }
        }

        private void EmitConversionExpression(NILProcessor processor, AbstractConversionExpression node)
        {
            EmitExpression(processor, node.Expression);      
            processor.Emit(InstructionOpcode.ConvType, _knownTypes[node.ResultType], node.OriginalNode.Span);
        }

        private void EmitCallExpression(NILProcessor processor, AbstractCallExpression node)
        {
            // Emit all the arguments
            foreach (AbstractExpression argument in node.Arguments)
                EmitExpression(processor, argument);

            InstructionOpcode callInstruction = node.IsAsync ? InstructionOpcode.Call_t : InstructionOpcode.Call;

            object arguments = node.Function.Name;
            if (!string.IsNullOrEmpty(node.Namespace))
            {
                arguments = new string[] { node.Namespace, node.Function.Name };
            }

            processor.Emit(callInstruction, arguments, node.OriginalNode.Span);
        }

        private void EmitBundleFieldAssignmentExpression(NILProcessor processor, AbstractBundleFieldAssignment node)
        {
            if (node.BundleVariable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[node.BundleVariable];
                EmitExpression(processor, node.Expression);
                processor.Emit(InstructionOpcode.Dup,
                    node.OriginalNode.Span); // Takes current value on stack and pushes it again

                processor.Emit(InstructionOpcode.StBArg,
                    new int[] { parameter.OrdinalPosition, node.FieldToAssign.OrdinalPosition },
                    node.OriginalNode.Span); // Writes value into local
                return;
            }

            VariableDefinition? variableDefinition = _currentContext.Locals[node.BundleVariable];
            EmitExpression(processor, node.Expression);
            processor.Emit(InstructionOpcode.Dup,
                node.OriginalNode.Span); // Takes current value on stack and pushes it again

            // TODO Add a concept of bundle fields in the emitter too
            processor.Emit(InstructionOpcode.StBloc,
                new int[] { variableDefinition.Index, node.FieldToAssign.OrdinalPosition },
                node.OriginalNode.Span); // Writes value into local
        }

        private void EmitAssignmentExpression(NILProcessor processor, AbstractAssignmentExpression node)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                ParameterDefinition? parameterDefinition = _currentContext.Parameters[node.Variable];
                EmitExpression(processor, node.Expression);
                processor.Emit(InstructionOpcode.Dup, node.OriginalNode.Span); // Takes current value on stack and pushes it again
                processor.Emit(InstructionOpcode.StArg, parameterDefinition, node.OriginalNode.Span); // Writes value into parameter
            }

            VariableDefinition? variableDefinition = _currentContext.Locals[node.Variable];
            EmitExpression(processor, node.Expression);
            processor.Emit(InstructionOpcode.Dup, node.OriginalNode.Span); // Takes current value on stack and pushes it again
            processor.Emit(InstructionOpcode.Stloc, variableDefinition, node.OriginalNode.Span); // Writes value into local
        }

        private void EmitVariableExpression(NILProcessor processor, AbstractVariableExpression node)
        {
            // TODO :: Rethink how variables vs bundles are handled to simplify the instruction set
            if (node.Variable is ParameterSymbol parameter)
            {
                InstructionOpcode paramOpcode = InstructionOpcode.Ldarg;
                object? paramArg = parameter.OrdinalPosition;
                if (node is AbstractBundleFieldAccessExpression fParam)
                {
                    paramOpcode = InstructionOpcode.LdBarg;
                    paramArg = new int[] { parameter.OrdinalPosition, fParam.Field.OrdinalPosition };
                }

                processor.Emit(paramOpcode, paramArg, node.OriginalNode.Span);
                return;
            }

            // TODO :: Figure out if we want to keep this variable definition as instruction argument or pass the index
            // directly

            VariableDefinition? variableDefinition = _currentContext.Locals[node.Variable];
            InstructionOpcode opcode = InstructionOpcode.Ldloc;
            object? argument = variableDefinition;
            if (node is AbstractBundleFieldAccessExpression f)
            {
                opcode = InstructionOpcode.LdBloc;
                argument = new int[] { variableDefinition.Index, f.Field.OrdinalPosition };
            }

            processor.Emit(opcode, argument, node.OriginalNode.Span);
        }

        private void EmitBinaryExpression(NILProcessor processor, AbstractBinaryExpression node)
        {
            // +(string, string)
            if (node.Operator.BinaryType == AbstractBinaryType.Addition)
            {
                if (node.Left.ResultType == TypeSymbol.String && node.Right.ResultType == TypeSymbol.String)
                {
                    //ilProcessor.Emit(OpCodes.Call, StringConcat2Reference);
                    EmitStringConcatExpression(processor, node);
                    return;
                }
            }

            EmitExpression(processor, node.Left);
            EmitExpression(processor, node.Right);

            // ==(string, string)
            // == (any, any)
            //if (node.Operator.BinaryType == BoundBinaryType.EQUALS)
            //{
            //    if (node.Left.ResultType == TypeSymbol.String && node.Right.ResultType == TypeSymbol.String ||
            //        node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any)
            //    {
            //        ilProcessor.Emit(OpCodes.Call, ObjectEqualsReference);
            //        return;
            //    }
            //}

            // !=(string, string)
            // !=(any, any)
            //if (node.Operator.BinaryType == BoundBinaryType.NOT_EQUALS)
            //{
            //    if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String ||
            //        node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any)
            //    {
            //        ilProcessor.Emit(OpCodes.Call, ObjectEqualsReference);
            //        ilProcessor.Emit(OpCodes.Ldc_I4_0);
            //        ilProcessor.Emit(OpCodes.Ceq);
            //        return;
            //    }
            //}

            switch (node.Operator.BinaryType)
            {
                case AbstractBinaryType.Addition:
                    processor.Emit(InstructionOpcode.Add, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.Subtraction:
                    processor.Emit(InstructionOpcode.Sub, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.Multiplication:
                    processor.Emit(InstructionOpcode.Mul, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.Division:
                    processor.Emit(InstructionOpcode.Div, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.Remainer:
                    processor.Emit(InstructionOpcode.Rem, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.LogicalAnd:
                case AbstractBinaryType.BitwiseAnd:
                    processor.Emit(InstructionOpcode.And, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.BitwiseOr:
                case AbstractBinaryType.LogicalOr:
                    processor.Emit(InstructionOpcode.Or, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.BitwiseXor:
                    processor.Emit(InstructionOpcode.Xor, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.Equals:
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.NotEquals:
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.LessThan:
                    processor.Emit(InstructionOpcode.Clt, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.LessThanOrEqual:
                    processor.Emit(InstructionOpcode.Cgt, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.GreaterThan:
                    processor.Emit(InstructionOpcode.Cgt, node.OriginalNode.Span);
                    break;
                case AbstractBinaryType.GreaterThanOrEqual:
                    processor.Emit(InstructionOpcode.Clt, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ldc_i4_0, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    break;
                default:
                    throw new Exception($"Unexpected binary operator {SyntaxEx.GetText(node.Operator.NodeType)}");
            }
        }

        private void EmitStringConcatExpression(NILProcessor processor, AbstractBinaryExpression node)
        {
            // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
            // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
            // Example: folding b and c in ((a + b) + c) if they are constant.

            List<AbstractExpression>? nodes = FoldConstants(node.OriginalNode, Flatten(node)).ToList();

            switch (nodes.Count)
            {
                case 0:
                    processor.Emit(InstructionOpcode.Ldc_s, string.Empty, node.OriginalNode.Span);
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
                        EmitExpression(processor, nodes[i]);
                    }

                    processor.Emit(InstructionOpcode.AddStr, nodes.Count, node.OriginalNode.Span);
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
                        yield return result;

                    foreach (AbstractExpression result in Flatten(binaryExpression.Right))
                        yield return result;
                }
                else
                {
                    if (node.ResultType != TypeSymbol.String)
                        throw new Exception($"Unexpected node type in string concatenation: {node.Type}");

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
                            continue;

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
                    yield return new AbstractLiteralExpression(syntax, sb.ToString());
            }

        }

        private void EmitUnaryExpression(NILProcessor processor, AbstractUnaryExpression node)
        {
            EmitExpression(processor, node.Operand);
            switch (node.Operator.UnaryType)
            {
                case AbstractUnaryType.Identity:
                    // NOP
                    break;
                case AbstractUnaryType.LogicalNegation:
                    processor.Emit(InstructionOpcode.Ldc_i4_0, node.OriginalNode.Span);
                    processor.Emit(InstructionOpcode.Ceq, node.OriginalNode.Span);
                    // !
                    break;
                case AbstractUnaryType.Negation:
                    processor.Emit(InstructionOpcode.Neg, node.OriginalNode.Span);
                    // -
                    break;
                case AbstractUnaryType.OnesComplement:
                    processor.Emit(InstructionOpcode.Not, node.OriginalNode.Span);
                    // ~
                    break;
                default:
                    throw new Exception($"Unexpected unary operator: ({SyntaxEx.GetText(node.Operator.NodeType)})({node.Operand.Type})");
            }
        }
        private static void EmitErrorExpression(NILProcessor processor, AbstractErrorExpression node) => throw new NotImplementedException();

        private static void EmitConstantExpression(NILProcessor processor, AbstractExpression node)
        {
            // A constant cannot be null here
            Debug.Assert(node.ConstantValue is not null);
            if (node.ResultType == TypeSymbol.Int)
            {
                int value = (int)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_i4, value, node.OriginalNode.Span);
                return;
            }

            if(node.ResultType == TypeSymbol.Float)
            {
                float value = (float)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_r4, value, node.OriginalNode.Span);
                return;
            }

            if (node.ResultType == TypeSymbol.Bool)
            {
                bool value = (bool)node.ConstantValue.Value;
                InstructionOpcode instruction = value ? InstructionOpcode.Ldc_i4_1 : InstructionOpcode.Ldc_i4_0;
                processor.Emit(instruction, node.OriginalNode.Span);
                return;
            }

            if (node.ResultType == TypeSymbol.String)
            {
                string? value = (string)node.ConstantValue.Value;
                processor.Emit(InstructionOpcode.Ldc_s, value, node.OriginalNode.Span);
                return;
            }

            throw new Exception($"Unexpected constant expression type: {node.Type}");
        }

        #endregion

        private static NativeAttribute GenerateAttributeMask(ImmutableArray<AttributeSymbol> attributes)
        {
            NativeAttribute mask = NativeAttribute.Uknown;
            foreach (AttributeSymbol attr in attributes)
            {
                if (!attr.IsMethodAttribute || attr.Attribute is null)
                    throw new InvalidDataException(nameof(attr.Attribute));

                mask |= (NativeAttribute)attr.Attribute;
            }

            return mask;
        }

        private readonly Dictionary<TypeSymbol, TypeReference> _knownTypes = new()
        {
            {TypeSymbol.Error, TypeReference.Unknown},
            {TypeSymbol.Void, TypeReference.Void},
            //{TypeSymbol.Char, TypeReference.Char},
            {TypeSymbol.Bool, TypeReference.Bool},
            {TypeSymbol.Int, TypeReference.Int},
            {TypeSymbol.Float, TypeReference.Float},
            {TypeSymbol.String, TypeReference.String},
            {TypeSymbol.Bundle, TypeReference.Bundle},
        };
    }
}
