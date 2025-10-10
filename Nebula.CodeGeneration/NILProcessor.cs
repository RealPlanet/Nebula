using Nebula.CodeGeneration.Definitions;
using Nebula.Commons.Syntax;
using Nebula.Interop.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.CodeGeneration
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

        public void Emit(InstructionOpcode opcode, Node? originalNode)
        {
            _instructions.Add(new Instruction(opcode, originalNode));
        }

        public void Emit(InstructionOpcode opcode, object argument, Node? originalNode)
        {
            _instructions.Add(new Instruction(opcode, argument, originalNode));
        }

        public void WriteComment(string trivia)
        {
            _instructions.Add(new TriviaInstruction(trivia));
        }
    }
}
