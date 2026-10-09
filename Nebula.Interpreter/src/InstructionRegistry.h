#pragma once

#ifndef _H_NEBULA_INSTRUCTION_REGISTRY_
#define _H_NEBULA_INSTRUCTION_REGISTRY_

#include "InstructionDefinitions.h"
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

#endif // !_H_NEBULA_INSTRUCTION_REGISTRY_

