﻿using Nebula.Commons.Text;
using System;
using System.Text;

namespace Nebula.CodeEmitter
{
    public interface IEmitterObject
    {
        /// <summary>The text location of this object, used to reference the source code original position</summary>
        TextSpan? SourceCodeTextSpan { get; }
    }

    public sealed class Instruction
        : IEmitterObject
    {
        public TextSpan? SourceCodeTextSpan { get; }
        public InstructionOpcode Opcode { get; set; }
        public object? Operand { get; set; } = null;

        public Instruction(InstructionOpcode opcode, TextSpan? sourceCodeTextSpan)
        {
            Opcode = opcode;
            SourceCodeTextSpan = sourceCodeTextSpan;
        }

        public Instruction(InstructionOpcode opcode, object operand, TextSpan? sourceCodeTextSpan)
            : this(opcode, sourceCodeTextSpan)
        {
            Operand = operand;
        }

        public string GetArgumentString()
        {
            return Opcode switch
            {
                InstructionOpcode.Ldc_s => $"\"{(string)Operand!}\"",
                InstructionOpcode.Ldc_i4 => ((int)Operand!).ToString(),
                InstructionOpcode.ConvType => Operand!.ToString()!,
                InstructionOpcode.Call or InstructionOpcode.Call_t => GetCallArgumentString(),
                InstructionOpcode.Stloc => ((VariableDefinition)Operand!).Index.ToString(),
                InstructionOpcode.StBloc => ProcessArrayOperand(),
                InstructionOpcode.StArg => ((ParameterDefinition)Operand!).Index.ToString(),
                InstructionOpcode.StBArg => ProcessArrayOperand(),
                InstructionOpcode.Ldarg => ((int)Operand!).ToString(),
                InstructionOpcode.LdBarg => ProcessArrayOperand(),
                InstructionOpcode.Ldloc => ((VariableDefinition)Operand!).Index.ToString(),
                InstructionOpcode.LdBloc => ProcessArrayOperand(),
                InstructionOpcode.Ld_b => GetLdBArgumentString(),
                InstructionOpcode.AddStr => ((int)Operand!).ToString(),
                InstructionOpcode.Br or InstructionOpcode.Brtrue or InstructionOpcode.Brfalse => Operand!.ToString()!,
                _ => string.Empty,
            };
        }

        private string GetLdBArgumentString()
        {
            if (Operand is string typeName)
                return typeName;

            if (Operand is string[] namespaceAndType)
            {
                return CreateSeparatedList(namespaceAndType, ' ');
            }

            throw new ArgumentException();
        }

        private string GetCallArgumentString()
        {
            if (Operand is string funcName)
                return funcName;

            if (Operand is string[] namespaceAndFunc)
            {
                return CreateSeparatedList(namespaceAndFunc, ' ');
            }

            throw new ArgumentException();
        }

        private string ProcessArrayOperand()
        {
            if (Operand is int[] array)
            {
                return CreateSeparatedList(array, ' ');
            }

            throw new ArgumentException();
        }

        private static string CreateSeparatedList(object[] arguments, char separator)
        {
            StringBuilder sb = new();
            foreach (object i in arguments)
            {
                sb.Append(i);
                sb.Append(separator);
            }

            return sb.ToString().Trim(separator);
        }

        private static string CreateSeparatedList(int[] arguments, char separator)
        {
            StringBuilder sb = new();
            foreach (int i in arguments)
            {
                sb.Append(i);
                sb.Append(separator);
            }

            return sb.ToString().Trim(separator);
        }
    }
}
