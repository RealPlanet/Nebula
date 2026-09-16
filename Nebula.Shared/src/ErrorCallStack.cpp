#include <sstream>
#include <format>
#include <fstream>
#include <string>

#include "ErrorCallStack.h"
#include "Utility.h"
#include "DebugServer.h"

using namespace nebula::shared;

static inline std::string get_line_at(std::ifstream& stream, size_t line, std::string& prev, std::string& next) {
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

ErrorCallStackLine::ErrorCallStackLine(const std::string& scriptSource, const std::string& namespace_, const std::string& funcName, size_t instNumber, const std::string& scriptText)
	: m_ScriptSource{ scriptSource }, m_Namespace{ namespace_ }, m_FunctionName{funcName}, m_InstructionNumber{instNumber}, m_ScriptLine{scriptText}
{
}

std::string ErrorCallStack::GetAsText() const
{
	std::stringstream ss;

	ss << "~~~~~~ Interpreter fatal error report: ~~~~~~\n";

	if (m_preText != "")
	{
		ss << "Error description: ";
		ss << m_preText << "\n";
	}

	const char* rootPrefix = "Root: ";
	size_t rootPrefixCount = cstrlen(rootPrefix);

	ss << "See callstack for error location: \n";

	for (size_t i{ 0 }; i < m_Lines.size(); i++)
	{
		auto& line = m_Lines[i];

		std::string strLine;
		if (GetErrorCallStackLineFromDbgFile(line, &strLine))
		{
			ss << strLine;
			continue;
		}


		if (i == m_Lines.size() - 1)
		{
			ss << rootPrefix + GetBytecodeErrorString(i);
			continue;
		}

		strLine = GetBytecodeErrorString(i);
		strLine.insert(0, rootPrefixCount, ' ');
		ss << strLine;
	}
	ss << "~~~~~~ End of Error ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n";

	return ss.str();
}

std::string ErrorCallStack::GetBytecodeErrorString(size_t lineIndex) const
{
	auto& line = m_Lines[lineIndex];
	std::string labelCountPrefix = std::format("+ @{}", line.GetInstructionNumber());
	if (lineIndex == m_Lines.size() - 1)
	{
		// Root of call stack
		return labelCountPrefix + "::" + line.GetText() + "\n";
	}

	return std::format("{}::{}\n", labelCountPrefix, line.GetText());
}

bool ErrorCallStack::GetErrorCallStackLineFromDbgFile(const ErrorCallStackLine& line, std::string* outTextLine) const
{
	if (debugger::DebugServer::Instance() == nullptr)
	{
		return false;
	}

	auto& namespace_ = line.GetFunctionNamespace();
	auto debugInformation = debugger::DebugServer::Instance()->GetScript(namespace_);
	if (debugInformation == nullptr)
	{
		return false;
	}

	auto& funcName = line.GetFunctionName();
	auto functionInformation = debugInformation->GetFunctionInformation(funcName);
	if (functionInformation == nullptr)
	{
		return false;
	}

	size_t instructionOpcode = line.GetInstructionNumber();
	size_t lineInfo = functionInformation->GetLineFromOpcode(instructionOpcode);
	if (lineInfo == debugger::symbols::DebugFunction::NoLineInfo)
	{
		return false;
	}

	std::stringstream ss;
	ss << std::format("At line {} in function '{}' of script '{}': \n",
		lineInfo, line.GetFunctionName(), debugInformation->originalFileName);

	std::ifstream f(debugInformation->originalFileFullName);
	if (f.is_open())
	{
		std::string prev, next;
		std::string lineText = get_line_at(f, lineInfo - 1, prev, next);
		if (next != "") {
			ss << "|\t" << prev << "\n";
			ss << "|\t" << lineText << "\n";
			ss << "L\t" << next << "\n";
		}
		else {
			ss << "|\t" << prev << "\n";
			ss << "L\t" << lineText << "\n";
		}
	}
	else {
		ss << "\t!Could not load load script text!\n";
	}
	f.close();

	*outTextLine = ss.str();
	return true;
}

std::string ErrorCallStack::GetReadableError() const
{
	return InstructionErrorCodeToString(m_ErrorCode);
}