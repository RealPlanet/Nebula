using Nebula.CodeGeneration.Definitions;
using Nebula.Interop.Enumerators;
using System;
using System.Collections.Generic;

namespace Nebula.CodeGeneration
{
    public delegate string InstructionArgumentGenerator(Instruction input);
    public static class InstructionArgumentFactory
    {
        private static readonly Dictionary<InstructionOpcode, InstructionArgumentGenerator> s_registeredGenerators = [];

        static InstructionArgumentFactory()
        {
            Register(InstructionOpcode.Nop, NoArgumentsAllowed);

            Register(InstructionOpcode.Ldc_s, RequireString);
            Register(InstructionOpcode.Ldc_i4, RequireInt32);
            Register(InstructionOpcode.Ldc_r4, RequireFloat);

            Register(InstructionOpcode.Ldarg, RequireVariableIndex);
            Register(InstructionOpcode.Ldloc, RequireVariableIndex);

            Register(InstructionOpcode.Stloc, RequireVariableIndex);
            Register(InstructionOpcode.StArg, RequireVariableIndex);

            Register(InstructionOpcode.AddStr, RequireInt32);

            Register(InstructionOpcode.Br, RequireInt32);
            Register(InstructionOpcode.BrFalse, RequireInt32);
            Register(InstructionOpcode.BrTrue, RequireInt32);

            Register(InstructionOpcode.ConvType, RequireTypeName);

            Register(InstructionOpcode.Call, ConvertGenericOperand);
            Register(InstructionOpcode.Call_t, ConvertGenericOperand);
            Register(InstructionOpcode.Callvirt, ConvertGenericOperand);

            Register(InstructionOpcode.Newobj, ConvertGenericOperand);
            Register(InstructionOpcode.Newarr, ConvertGenericOperand);

            Register(InstructionOpcode.StBloc, ConvertGenericOperand);
            Register(InstructionOpcode.StBArg, ConvertGenericOperand);
            Register(InstructionOpcode.LdBarg, ConvertGenericOperand);
            Register(InstructionOpcode.LdBloc, ConvertGenericOperand);
        }

        #region Generator callbacks
        private static string ConvertGenericOperand(Instruction input)
        {
            if (input.Operand is string str)
            {
                return str;
            }

            if (input.Operand is string[] strArr)
            {
                return strArr.Join(' ');
            }

            if (input.Operand is int[] intArray)
            {
                return intArray.Join(' ');
            }

            throw new InvalidOperationException($"Unknown operand type '{input.Operand?.GetType().Name ?? "NULL"}'");
        }

        private static string RequireTypeName(Instruction input)
        {
            if (input.Operand is TypeReference type)
            {
                return type.Name;
            }

            throw new InvalidOperationException($"Unknown operand type '{input.Operand?.GetType().Name ?? "NULL"}'");
        }

        private static string RequireVariableIndex(Instruction input)
        {
            if (input.Operand is VariableDefinition varDef)
            {
                return varDef.Index.ToString();
            }

            if (input.Operand is ParameterDefinition parDef)
            {
                return parDef.Index.ToString();
            }

            throw new InvalidOperationException($"Unknown operand type '{input.Operand?.GetType().Name ?? "NULL"}'");
        }

        private static string RequireInt32(Instruction input)
        {
            return Require<int>(input.Operand).ToString();
        }

        private static string RequireFloat(Instruction input)
        {
            return Require<float>(input.Operand).ToString();
        }

        private static string RequireString(Instruction input)
        {
            if (input.Operand == null)
            {
                throw new InvalidOperationException("This operation does requires arguments!");
            }

            var str = Require<string>(input.Operand);
            return $"\"{str}\"";
        }

        private static string NoArgumentsAllowed(Instruction input)
        {
            if (input.Operand != null)
            {
                throw new InvalidOperationException("This operation does not allow arguments!");
            }

            return string.Empty;
        }

        #endregion

        public static void Register(InstructionOpcode opcode, InstructionArgumentGenerator callback)
        {
            s_registeredGenerators.Add(opcode, callback);
        }

        public static T Require<T>(object? op)
            where T : notnull
        {
            if (op is not T casted)
            {
                throw new InvalidOperationException($"Expected operand of type {typeof(T).Name} but got {op?.GetType().Name ?? "null"}");
            }

            return casted;
        }

        public static string GetOpcodeArguments(Instruction instruction)
        {
            if (s_registeredGenerators.TryGetValue(instruction.Opcode, out var generator))
            {
                return generator.Invoke(instruction);
            }

            return string.Empty;
        }
    }
}
