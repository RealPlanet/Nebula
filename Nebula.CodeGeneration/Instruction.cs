using Nebula.Commons.Syntax;
using Nebula.Interop.Enumerators;
using System.Collections.Generic;

namespace Nebula.CodeGeneration
{
    public class Instruction
        : IEmitterObject
    {
        public Node? OriginalNode { get; }
        public InstructionOpcode Opcode { get; set; }
        public object? Operand { get; set; } = null;
        public HashSet<string> LeadingComments { get; } = [];

        public Instruction(InstructionOpcode opcode, Node? originalNode)
        {
            Opcode = opcode;
            OriginalNode = originalNode;
        }

        public Instruction(InstructionOpcode opcode, object operand, Node? originalNode)
            : this(opcode, originalNode)
        {
            Operand = operand;
        }
    }
}
