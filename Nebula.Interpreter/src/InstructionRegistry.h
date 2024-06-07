#pragma once

#include "LanguageTypes.h"
#include "InstructionDefs.h"
#include "Instruction.h"

namespace nebula
{
    class Frame;
    class Interpreter;
}

namespace nebula
{
    InstructionArguments	GenerateArgumentsForOpcode(VMInstruction, const RawArguments&);
    InstructionErrorCode	ExecuteInstruction(VMInstruction, Interpreter*, Frame*, const InstructionArguments&);
}

