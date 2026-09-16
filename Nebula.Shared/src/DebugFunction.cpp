#include "DebugFunction.h"

using namespace nebula::debugger::symbols;

size_t DebugFunction::GetLineFromOpcode(size_t opcode) const
{
	if (opcode < 0)
	{
		return NoLineInfo;
	}

	size_t lastPossibleLine = 0;
	for (auto& kvp : lines)
	{
		size_t lineno = kvp.lineNumber;
		size_t startingOpcodeOfLine = kvp.firstOpcodeOfLine;

		if (startingOpcodeOfLine == opcode) {
			return lineno;
		}

		if (startingOpcodeOfLine > opcode) {
			return lastPossibleLine;
		}

		lastPossibleLine = lineno;
	}

	return NoLineInfo;
}

size_t DebugFunction::GetOpcodeFromLine(size_t targetLine) const
{
	size_t count = 0;
	size_t lastOpcode{ 0 };
	for (auto& line : lines)
	{
		auto lineNo = line.lineNumber;
		if (lineNo == targetLine)
		{
			return line.firstOpcodeOfLine;
		}

		count++;
		if (lineNo <= targetLine)
		{
			lastOpcode = line.firstOpcodeOfLine;
			continue;
		}

		break;
	}

	return lastOpcode;
}
