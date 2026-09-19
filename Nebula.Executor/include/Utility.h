#pragma once

#ifndef _H_EXECUTOR_DBG_UTILITY_
#define _H_EXECUTOR_DBG_UTILITY_

#include <cstdint>
#include <filesystem>
#include <string>
#include <string_view>
#include <charconv>

#include "DAPTypes.h"
#include "DAPEvents.h"
#include "DebugTypes.h"
#include "ErrorCallstack.h"

namespace nebula::utility
{
	std::uint64_t GetProcessPID();
	bool IsProcessActive(std::uint64_t pid);

	void WriteDebuggerFile(int32_t port);
	void CleanupDebuggerFiles(bool includeSelf);

	std::string ReadAllText(const std::filesystem::path& path);

	void assign(::dap::Source& source, const nebula::debugger::DebugSource& debugSource);
	void assign(::dap::Scope& scope, const nebula::debugger::DebugScope& debugScope);
	void assign(::dap::Variable& variable, const nebula::debugger::DebugVariable& debugVariable);

	std::vector<nebula::debugger::DebugOutput> build_fatal_error_outputs(const nebula::shared::ErrorCallStack* callstack);
	std::string load_script_line_from_source(const std::string& path, size_t line);
	std::string get_line_at(std::ifstream& stream, size_t line);

	bool try_parse(std::string_view str, size_t& result);
	bool try_parse(std::string_view str, double& result);

	void to_lower_implace(std::string& str);
	std::string to_lower(std::string str);
}

#endif // !_H_EXECUTOR_DBG_UTILITY_
