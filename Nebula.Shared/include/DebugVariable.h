#pragma once

#ifndef _H_DEBUG_VARIABLE_
#define _H_DEBUG_VARIABLE_

#include <string>

namespace nebula::debugger::symbols
{
	struct DebugVariable
	{
		std::string name{};
		std::string sourceNamespace{};
		std::string sourceType{};
	};
} // namespace nebula::debugger

#endif // !_H_VARIABLE_DBG_DATA-
