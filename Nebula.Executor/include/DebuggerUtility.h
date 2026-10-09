#pragma once

#ifndef _H_EXECUTOR_DBG_UTILITY_
#define _H_EXECUTOR_DBG_UTILITY_

#include <cstdint>
#include <filesystem>
#include <string>
#include <string_view>
#include <charconv>

#include "DebuggerEntities.h"
#include "DAPTypes.h"
#include "ExceptionCallstack.h"

namespace nebula::debugger::utility
{
	std::uint64_t GetProcessPID();
	bool IsProcessActive(std::uint64_t pid);

	void WriteDebuggerFile(int32_t port);
	void CleanupDebuggerFiles(bool includeSelf);

	std::string ReadAllText(const std::filesystem::path& path);

	void assign(::dap::Source& source, const nebula::debugger::Source& debugSource);
	void assign(::dap::Scope& scope, const nebula::debugger::GenericScope& debugScope);
	void assign(::dap::Variable& variable, const nebula::debugger::Value& debugValue);

	std::vector<nebula::debugger::InterpreterOutput> build_fatal_error_outputs(const nebula::shared::ExceptionCallstack* callstack);
	std::string load_script_line_from_source(const std::string& path, size_t line);
	std::string get_line_at(std::ifstream& stream, size_t line);

	bool try_parse(std::string_view str, size_t& result);
	bool try_parse(std::string_view str, double& result);

	void to_lower_implace(std::string& str);
	std::string to_lower(std::string str);
}

#endif // !_H_EXECUTOR_DBG_UTILITY_
