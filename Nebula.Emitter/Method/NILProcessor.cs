
using Nebula.Commons.Text;
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

        public void Emit(InstructionOpcode opcode, TextSpan? sourceCodeLocation)
        {
            _instructions.Add(new(opcode, sourceCodeLocation));
        }

        public void Emit(InstructionOpcode opcode, object argument, TextSpan? sourceCodeLocation)
        {
            _instructions.Add(new(opcode, argument, sourceCodeLocation));
        }
    }
}
