using Nebula.CodeGeneration.DebugSymbols;
using Nebula.CodeGeneration.Definitions;
using Nebula.Commons.Text;
using Nebula.Interop.Enumerators;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nebula.CodeGeneration.Writer
{
    internal static class AssemblyWriter
    {
        public static void WriteAssembly(this StreamWriter writer, Assembly assembly)
        {
            IndentedTextWriter inWriter = new(writer);

            inWriter.WriteComment($">> {assembly.ModuleName} - Version {assembly.Version} <<");
            inWriter.WriteLine();
            inWriter.WriteNamespace(assembly.Namespace);
            inWriter.WriteLine();

            inWriter.WriteGlobals(assembly.TypeDefinition.Globals);

            foreach (ClassDefinition bundle in assembly.TypeDefinition.Classes)
            {
                inWriter.WriteBundle(bundle);
            }

            foreach (MethodDefinition func in assembly.TypeDefinition.Methods)
            {
                inWriter.WriteMethod(func);
            }
        }

        public static void WriterDebuggingInfo(this StreamWriter writer, Assembly assembly, string assemblyChecksum)
        {
            DebugSymbolsFile debugSymbols = new()
            {
                Namespace = assembly.Namespace,
                SourceFilePath = assembly.SourceCode.FullPath,
                MD5Hash = assemblyChecksum,
            };

            TypeInterner interner = new(debugSymbols);

            foreach (ClassDefinition clazz in assembly.TypeDefinition.Classes)
            {
                interner.RegisterClass(clazz, assembly.Namespace);
            }

            foreach(VariableDefinition global in assembly.TypeDefinition.Globals)
            {
                int typeId = interner.GetOrCreateTypeId(global.VariableType);
                debugSymbols.Globals.Add(new VariableDebugSymbol
                {
                    Name = global.Name,
                    TypeId = typeId,
                });
            }

            foreach (NativeMethodDefinition nativeFunc in assembly.TypeDefinition.NativeMethods)
            {
                debugSymbols.NativeFunctions.Add(nativeFunc.Name);
            }

            foreach (MethodDefinition func in assembly.TypeDefinition.Methods)
            {
                int funcLineNumber = -1;
                int funcEndLineNumber = -1;
                if (func.OriginalNode != null)
                {
                    funcLineNumber = assembly.SourceCode.GetLineIndex(func.OriginalNode.Span.Start);
                    funcEndLineNumber = assembly.SourceCode.GetLineIndex(func.OriginalNode.Span.End);
                }

                FunctionSymbol dbgFunc = new()
                {
                    Name = func.Name,
                    LineNumber = funcLineNumber,
                    InstructionCount = func.Body.Instructions.Count,
                    EndLineNumber = funcEndLineNumber,
                };

                debugSymbols.Functions.Add(dbgFunc.Name, dbgFunc);

                foreach (ParameterDefinition p in func.Parameters)
                {
                    dbgFunc.Parameters.Add(new()
                    {
                        Name = p.Name,
                        TypeId = interner.GetOrCreateTypeId(p.VariableType),
                    });
                }

                foreach (VariableDefinition v in func.Body.Variables)
                {
                    dbgFunc.LocalVariables.Add(new VariableDebugSymbol
                    {
                        Name = v.Name,
                        TypeId = interner.GetOrCreateTypeId(v.VariableType),
                    });
                }

                int lastLineNumber = -1;
                for (int i = 0; i < func.Body.Instructions.Count; i++)
                {
                    Instruction inst = func.Body.Instructions[i];
                    TextSpan instSpan = inst.OriginalNode?.Span ?? default;
                    int lineNumber = assembly.SourceCode.GetLineIndex(instSpan.Start);
                    if (lineNumber != lastLineNumber)
                    {
                        dbgFunc.Lines.Add(new(lineNumber, i));
                        lastLineNumber = lineNumber;
                    }
                }
            }

            writer.Write(JsonSerializer.Serialize(debugSymbols, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        public static void WriteComment(this IndentedTextWriter writer, string comment)
        {
            writer.Write(InterpreterWords.GetTokenChar(TokenType.CompiledComment));
            writer.WriteSpace();
            writer.WriteLine(comment);
        }

        public static void WriteNamespace(this IndentedTextWriter writer, string _namespace)
        {
            writer.Write(InterpreterWords.GetScriptSectionName(ScriptSection.Namespace, true));
            writer.WriteSpace();
            writer.WriteLine($"\"{_namespace}\"");
        }

        public static void WriteBundle(this IndentedTextWriter writer, ClassDefinition bundle)
        {
            writer.Write(InterpreterWords.GetScriptSectionName(ScriptSection.Bundle, true));
            writer.WriteSpace();
            writer.Write(bundle.Name);

            writer.WriteMethodParameters(bundle.Fields);

            writer.WriteLine();
        }

        public static void WriteMethod(this IndentedTextWriter writer, MethodDefinition method)
        {
            writer.Write(InterpreterWords.GetScriptSectionName(ScriptSection.Function, true));
            writer.WriteSpace();
            writer.Write(method.ReturnType.Name.ToLower());
            writer.WriteSpace();
            writer.Write(method.Name);
            writer.WriteMethodParameters(method.Parameters);
            writer.WriteSpace();
            writer.WriteAttributes(method.Attributes);
            writer.OpenScope();

            writer.WriteLocals(method.Body.Variables);

            int instCount = 0;
            foreach (Instruction inst in method.Body.Instructions)
            {
                writer.WriteInstruction(inst, instCount++);
            }

            writer.CloseScope();
            writer.WriteLine();
        }

        public static void WriteGlobals(this IndentedTextWriter writer, ICollection<VariableDefinition> variables)
        {
            string marker = InterpreterWords.GetScriptSectionName(ScriptSection.Globals, true);
            writer.Write(marker);
            writer.WriteSpace();
            writer.Write("[ ");

            int i = 0;
            foreach (var variable in variables)
            {
                writer.WriteVariable(variable, withName: true);

                if (i != variables.Count - 1)
                {
                    writer.Write(", ");
                }

                i++;
            }

            writer.WriteLine(" ]");
        }

        public static void WriteLocals(this IndentedTextWriter writer, IList<VariableDefinition> variables)
        {
            string marker = InterpreterWords.GetScriptSectionName(ScriptSection.Locals, true);
            writer.Write(marker);
            writer.WriteSpace();
            writer.Write("[ ");

            for (int i = 0; i < variables.Count; i++)
            {
                VariableDefinition variable = variables[i];
                writer.WriteVariable(variable, withName: false);

                if (i != variables.Count - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.WriteLine(" ]");
        }

        public static void WriteMethodParameters(this IndentedTextWriter writer, IList<ParameterDefinition> parameters)
        {
            string open = InterpreterWords.GetTokenChar(TokenType.OpenParenthesis);
            string close = InterpreterWords.GetTokenChar(TokenType.ClosedParenthesis);

            writer.Write(open + " ");

            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterDefinition? parameter = parameters[i];
                writer.WriteParameter(parameter);

                if (i != parameters.Count - 1)
                {
                    writer.Write(" , ");
                }
            }

            writer.Write(" " + close);
        }

        public static void WriteType(this IndentedTextWriter writer, TypeReference type)
        {
            writer.Write(type.CompiledName);
        }

        public static void WriteParameter(this IndentedTextWriter writer, ParameterDefinition param)
        {
            writer.WriteType(param.VariableType);
            writer.WriteSpace();
            writer.Write(param.Name);
        }

        public static void WriteVariable(this IndentedTextWriter writer, VariableDefinition param, bool withName)
        {
            if (withName)
            {
                writer.Write(param.Name);
                writer.Write(" : ");
            }

            writer.WriteType(param.VariableType);
        }

        public static void WriteInstruction(this IndentedTextWriter writer, Instruction instruction, int labelCount)
        {
            if (instruction is TriviaInstruction t)
            {
                foreach (var c in t.LeadingComments)
                {
                    writer.WriteComment(c);
                }

                return;
            }

            writer.WriteLabel(labelCount);
            writer.WriteSpace();
            writer.WriteInstruction(instruction);
            writer.WriteLine();
        }

        public static void WriteLabel(this IndentedTextWriter writer, int labelCount)
        {
            writer.Write(labelCount.ToString("X4"));
        }

        public static void WriteAttributes(this IndentedTextWriter writer, AttributeType attrs)
        {
            if ((attrs & AttributeType.AutoExec) == AttributeType.AutoExec)
            {
                string attrPrefix = InterpreterWords.GetTokenChar(TokenType.AttributePrefix);
                writer.Write(attrPrefix);
                writer.Write(nameof(AttributeType.AutoExec).ToLower());
                writer.Write(' ');
            }

            if ((attrs & AttributeType.AutoGenerated) == AttributeType.AutoGenerated)
            {
                string attrPrefix = InterpreterWords.GetTokenChar(TokenType.AttributePrefix);
                writer.Write(attrPrefix);
                writer.Write(nameof(AttributeType.AutoGenerated).ToLower());
                writer.Write(' ');
            }

            if ((attrs & AttributeType.Initializer) == AttributeType.Initializer)
            {
                string attrPrefix = InterpreterWords.GetTokenChar(TokenType.AttributePrefix);
                writer.Write(attrPrefix);
                writer.Write(nameof(AttributeType.Initializer).ToLower());
                writer.Write(' ');
            }

            writer.WriteLine();
        }

        public static void WriteSpace(this IndentedTextWriter writer)
        {
            writer.Write(" ");
        }

        public static void OpenScope(this IndentedTextWriter writer)
        {
            string openBody = InterpreterWords.GetTokenChar(TokenType.OpenBody);
            writer.WriteLine(openBody);
            writer.Indent++;
        }

        public static void CloseScope(this IndentedTextWriter writer)
        {
            string closeBody = InterpreterWords.GetTokenChar(TokenType.CloseBody);
            writer.Indent--;
            writer.WriteLine(closeBody);
        }
    }
}
