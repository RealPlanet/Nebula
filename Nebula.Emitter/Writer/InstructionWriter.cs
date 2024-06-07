using System.CodeDom.Compiler;

namespace Nebula.CodeEmitter.Writer
{
    internal static class InstructionWriter
    {
        public static void WriteInstruction(this IndentedTextWriter writer, Instruction instruction)
        {
            writer.Write(instruction.Opcode.ToString().ToLower());
            writer.WriteSpace();
            writer.Write(instruction.GetArgumentString());
        }
    }
}
