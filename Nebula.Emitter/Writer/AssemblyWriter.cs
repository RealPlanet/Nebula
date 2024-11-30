﻿using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Nebula.CodeEmitter.Writer
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

            foreach (BundleDefinition bundle in assembly.TypeDefinition.Bundles)
            {
                inWriter.WriteBundle(bundle);
            }

            foreach (MethodDefinition func in assembly.TypeDefinition.Methods)
            {
                inWriter.WriteMethod(func);
            }
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

        public static void WriteBundle(this IndentedTextWriter writer, BundleDefinition bundle)
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

        public static void WriteLocals(this IndentedTextWriter writer, IList<VariableDefinition> variables)
        {
            string marker = InterpreterWords.GetScriptSectionName(ScriptSection.Locals, true);
            writer.Write(marker);
            writer.WriteSpace();
            writer.Write("[ ");

            for (int i = 0; i < variables.Count; i++)
            {
                VariableDefinition variable = variables[i];
                writer.WriteVariable(variable);

                if (i != variables.Count - 1)
                    writer.Write(", ");
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
                    writer.Write(" , ");
            }

            writer.Write(" " + close);
        }

        public static void WriteParameter(this IndentedTextWriter writer, ParameterDefinition param)
        {
            writer.Write(param.ParameterType.Name.ToLower());
            writer.WriteSpace();
            writer.Write(param.Name);
        }

        public static void WriteVariable(this IndentedTextWriter writer, VariableDefinition param)
        {
            writer.Write(param.VariableType.Name.ToLower());
            //writer.WriteSpace();
            //writer.Write(param.Index);
        }

        public static void WriteInstruction(this IndentedTextWriter writer, Instruction instruction, int labelCount)
        {
            writer.WriteLabel(labelCount);
            writer.WriteSpace();
            writer.WriteInstruction(instruction);
            writer.WriteLine();
        }

        public static void WriteLabel(this IndentedTextWriter writer, int labelCount)
        {
            writer.Write(labelCount.ToString("X4"));
        }

        public static void WriteAttributes(this IndentedTextWriter writer, NativeAttribute attrs)
        {
            string attrPrefix = InterpreterWords.GetTokenChar(TokenType.AttributePrefix);
            if ((attrs & NativeAttribute.AutoExec) == NativeAttribute.AutoExec)
            {
                writer.Write(attrPrefix);
                writer.Write(nameof(NativeAttribute.AutoExec).ToLower());
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
