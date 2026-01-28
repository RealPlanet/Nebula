using Nebula.Shared.Enumerators;

namespace Nebula.CodeGeneration
{
    public class TriviaInstruction
        : Instruction
    {
        public TriviaInstruction(string comment)
            : base(InstructionOpcode.Nop, null)
        {
            LeadingComments.Add(comment);
        }
    }
}
