using Nebula.Shared.Enumerators;

namespace Nebula.Interop.Structures
{
    public sealed class Instruction
    {
        public InstructionOpcode Opcode { get; }
        public Instruction(InstructionOpcode opcode)
        {
            Opcode = opcode;
        }
    }
}
