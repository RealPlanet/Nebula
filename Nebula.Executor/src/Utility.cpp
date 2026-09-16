#include "DebugServer.h"
#include "DebugTypes.h"
#include "Utility.h"

#include <cassert>
#include <filesystem>
#include <fstream>
#include <string>
#include <cstdint>
#include <DAPTypes.h>
#include <DebugFunction.h>
#include <ErrorCallStack.h>
#include <exception>
#include <format>
#include <sstream>
#include <stdexcept>
#include <system_error>
#include <vector>

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <process.h>
#include <Windows.h>
#endif

std::uint64_t nebula::utility::GetProcessPID()
{
#ifdef _WIN32
	return static_cast<std::uint64_t>(::_getpid());
#else
	return static_cast<std::uint64_t>(::getpid());
#endif
}

bool nebula::utility::IsProcessActive(std::uint64_t pid)
{
#ifdef _WIN32
	HANDLE process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, static_cast<DWORD>(pid));

	if (process == nullptr)
		return GetLastError() != ERROR_INVALID_PARAMETER;

	DWORD exitCode = 0;

	bool active = GetExitCodeProcess(process, &exitCode) && exitCode == STILL_ACTIVE;

	CloseHandle(process);

	return active;

#else
	return kill(pid, 0) == 0 || errno == EPERM;
#endif
}

void nebula::utility::WriteDebuggerFile(int32_t port)
{
	int64_t pid = nebula::utility::GetProcessPID();
	std::ofstream output{ "." + std::to_string(pid) };
	output << "nebula_dbg\n";
	output << port;
	output.close();
}

void nebula::utility::CleanupDebuggerFiles(bool includeSelf)
{
	namespace fs = std::filesystem;

	for (const auto& entry : fs::directory_iterator("."))
	{
		if (!entry.is_regular_file())
		{
			continue;
		}

		auto filename = entry.path().filename().string();
		if (!filename.starts_with('.'))
		{
			continue;
		}

		filename = filename.substr(1);
		int pid = 0;
		try
		{
			size_t parsedCharacters = 0;
			pid = std::stoi(filename, &parsedCharacters);

			if (parsedCharacters != filename.size())
			{
				continue;
			}
		}
		catch (const std::exception&)
		{
			continue;
		}

		{
			// Scope to close the file after reading
			std::ifstream input(entry.path());
			if (!input)
			{
				continue;
			}

			std::string identifier;
			std::getline(input, identifier);

			// Make sure this is actually one of our debugger files.
			if (identifier != "nebula_dbg")
			{
				continue;
			}
		}

		if (!IsProcessActive(pid) || (includeSelf && pid == GetProcessPID()))
		{
			std::error_code error;
			fs::remove(entry.path(), error);
		}
	}
}

std::string nebula::utility::ReadAllText(const std::filesystem::path& path)
{
	std::ifstream file{ path };
	if (!file.is_open())
	{
		throw std::runtime_error("Could not open file.");
	}

	std::stringstream sb;
	sb << file.rdbuf();
	file.close();
	return sb.str();
}

void nebula::utility::assign(::dap::Source& source, const nebula::debugger::DebugSource& debugSource)
{
	source.name = debugSource.name;
	source.path = debugSource.path;
	if (debugSource.reference > 0)
	{
		source.sourceReference = debugSource.reference;
	}

	source.presentationHint = ::dap::Source::PresentationHintValues::Normal;
	source.origin = ::dap::Source::Origin{ debugSource.origin };
}

void nebula::utility::assign(::dap::Scope& scope, const nebula::debugger::DebugScope& debugScope)
{
	scope.name = debugScope.name;
	scope.variablesReference = debugScope.reference;
	scope.expensive = false;

	switch (debugScope.type)
	{
	case nebula::debugger::ScopeType::Generic:
	{
		scope.presentationHint = ::dap::Scope::PresentationHint{ "Generic" };
		break;
	}
	case nebula::debugger::ScopeType::Arguments:
	{
		scope.presentationHint = ::dap::Scope::PresentationHint{ "Arguments" };
		break;
	}
	case nebula::debugger::ScopeType::Locals:
	{
		scope.presentationHint = ::dap::Scope::PresentationHint{ "Locals" };
		break;
	}
	default:
		assert(false);
	}
}

void nebula::utility::assign(::dap::Variable& variable, const nebula::debugger::DebugVariable& debugVariable)
{
	variable.name = debugVariable.name;
	variable.evaluateName = debugVariable.name;
	variable.value = debugVariable.value;
	variable.type = "";
	variable.variablesReference = debugVariable.reference;
	variable.presentationHint = ::dap::VariablePresentationHint
	{
		.visibility = ::dap::VariablePresentationHint::VisibilityValues::Public,
	};

	if (debugVariable.generalScope == nebula::debugger::DebugVariable::Scope::Local)
	{
		variable.presentationHint.value().kind = ::dap::VariablePresentationHint::KindValues::Locals;
	}
	else if (debugVariable.generalScope == nebula::debugger::DebugVariable::Scope::Arguments)
	{
		variable.presentationHint.value().kind = ::dap::VariablePresentationHint::KindValues::Arguments;
	}
	else if (debugVariable.generalScope == nebula::debugger::DebugVariable::Scope::Global)
	{
		variable.presentationHint.value().kind = ::dap::VariablePresentationHint::Kind{ "Global" };
	}

	// TODO
	//switch (debugVariable.internalType)
	//{
	//case nebula::debugger::DebugVariable::Type::Array:
	//{
	//
	//}
	//}
}

std::vector<nebula::debugger::DebugOutput> nebula::utility::build_fatal_error_outputs(const nebula::shared::ErrorCallStack* callstack)
{
	std::vector<nebula::debugger::DebugOutput> result;

	auto dbgServer = debugger::DebugServer::Instance();
	if (!dbgServer)
	{
		result.push_back(nebula::debugger::DebugOutput{
				.output = "Cannot load fatal error, debug server not found."
			});
		return result;
	}

	for (size_t i{ 0 }; i < callstack->GetLineCount(); i++)
	{
		auto& line = callstack->LineAt(i);

		auto debugSymbols = dbgServer->GetScript(line.GetFunctionNamespace());
		if (!debugSymbols)
		{
			result.push_back(nebula::debugger::DebugOutput{
					.output = callstack->GetBytecodeErrorString(i),
					.functionNamespace = line.GetFunctionNamespace(),
					.functionName = line.GetFunctionName(),
					.line = debugger::symbols::DebugFunction::NoLineInfo,
				});
			continue;
		}

		auto funcDebugSymbols = debugSymbols->GetFunctionInformation(line.GetFunctionName());
		if (!funcDebugSymbols)
		{
			result.push_back(nebula::debugger::DebugOutput{
					.output = callstack->GetBytecodeErrorString(i),
					.functionNamespace = line.GetFunctionNamespace(),
					.functionName = line.GetFunctionName(),
					.line = debugger::symbols::DebugFunction::NoLineInfo,
					.fileSymbols = debugSymbols,
				});
			continue;
		}

		size_t instructionOpcode = line.GetInstructionNumber();
		size_t lineInfo = funcDebugSymbols->GetLineFromOpcode(instructionOpcode);
		if (lineInfo == debugger::symbols::DebugFunction::NoLineInfo)
		{
			result.push_back(nebula::debugger::DebugOutput{
					.output = callstack->GetBytecodeErrorString(i),
					.functionNamespace = line.GetFunctionNamespace(),
					.functionName = line.GetFunctionName(),
					.line = debugger::symbols::DebugFunction::NoLineInfo,
					.fileSymbols = debugSymbols,
					.functionSymbols = funcDebugSymbols,
				});
			continue;
		}

		auto event = nebula::debugger::DebugOutput
		{
			.output = std::format("[{}]!{}::{}", line.GetScriptSource(), line.GetFunctionNamespace(), line.GetFunctionName()),
			.functionNamespace = line.GetFunctionNamespace(),
			.functionName = line.GetFunctionName(),
			.line = lineInfo,
			.fileSymbols = debugSymbols,
			.functionSymbols = funcDebugSymbols,
		};

		if (i == 0)
		{
			event.output += std::format("\n{}", load_script_line_from_source(debugSymbols->originalFileFullName, lineInfo));
		}

		result.push_back(event);
	}

	return result;
}

std::string nebula::utility::load_script_line_from_source(const std::string& path, size_t line)
{
	std::ifstream f(path);
	if (!f.is_open())
	{
		return "## Could not read script line from file! ##\n";
	}

	return get_line_at(f, line);
}

std::string nebula::utility::get_line_at(std::ifstream& stream, size_t line)
{
	std::string prev, next;
	while (line-- > 0) {
		std::getline(stream, prev);
	}

	std::string l;
	std::getline(stream, l);
	if (!stream.eof()) {
		std::getline(stream, next);
	}
	else {
		next = "";
	}

	return l;
}
