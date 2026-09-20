#pragma once

#ifndef _H_DEBUG_FILE_
#define _H_DEBUG_FILE_

#include <memory>
#include <string>
#include <unordered_set>
#include <unordered_map>

#include "DebugFunction.h"
#include "DebugBundleDefinition.h"

namespace nebula::debugger::symbols
{
	struct DebugFile
	{
		std::string namespace_{};
		// Original source file name
		std::string originalFileName{};
		// Full path of source file
		std::string originalFileFullName{};
		std::string md5Hash{};

		std::unordered_map<std::string, DebugBundleDefinition> types{};
		std::unordered_map<std::string, DebugFunction> functions{};
		std::unordered_set<std::string> nativeFunctions{};

		const DebugFunction* GetFunctionInformation(const std::string& funcName) const;
	};

	using ScriptDbgDataPtr = std::unique_ptr<DebugFile>;
}

#endif // !_H_SCRIPT_DBG_INFORMATION_
