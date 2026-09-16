#pragma once

#ifndef _H_DEBUG_FUNCTION_
#define _H_DEBUG_FUNCTION_

#include "DebugLine.h"
#include "DebugVariable.h"

#include <string>
#include <vector>

namespace nebula::debugger::symbols
{
	struct DebugFunction
	{
	public:
		static const size_t NoLineInfo{ (size_t)-1 };

		std::string name{ };
		// 0-based line number
		size_t lineNumber{ 0 };
		// 0-based line number
		size_t endLineNumber{ 0 };
		size_t instructionCount{ 0 };
		std::vector<DebugVariable> parameters{};
		std::vector<DebugVariable> locals{};
		std::vector<DebugLine> lines{};
		std::vector<int> statements{};

	public:
		size_t GetLineFromOpcode(size_t opcode) const;
		size_t GetOpcodeFromLine(size_t line) const;
	};
} // namespace nebula::debugger

#endif // !_H_FUNCTION_DBG_INFORMATION_
