using Nebula.Interop.Enumerators;

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
