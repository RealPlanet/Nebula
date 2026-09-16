#pragma once

#ifndef _H_DEBUG_LINE_
#define _H_DEBUG_LINE_

namespace nebula::debugger::symbols
{
	struct DebugLine
	{
		size_t lineNumber{};
		size_t firstOpcodeOfLine{};
	};
} // namespace nebula::debugger

#endif // !_H_LINE_DBG_INFORMATION_
