
using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    public sealed class NILProcessor
    {
        public MethodBody Body { get; }
        private readonly ICollection<Instruction> _instructions;

        public NILProcessor(MethodBody methodBody)
        {
            Body = methodBody;
            _instructions = methodBody.Instructions;
        }

        public void Emit(InstructionOpcode opcode)
        {
            _instructions.Add(new(opcode));
        }

        public void Emit(InstructionOpcode opcode, object argument)
        {
            _instructions.Add(new(opcode, argument));
        }
    }
}
