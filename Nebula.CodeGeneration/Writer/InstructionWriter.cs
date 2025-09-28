using System.CodeDom.Compiler;

namespace Nebula.CodeGeneration.Writer
{
    internal static class InstructionWriter
    {
        public static void WriteInstruction(this IndentedTextWriter writer, Instruction instruction)
        {
            writer.Write(instruction.Opcode.ToString().ToLower());
            writer.WriteSpace();
            string arguments = InstructionArgumentFactory.GetOpcodeArguments(instruction);
            writer.Write(arguments);
        }
    }
}
