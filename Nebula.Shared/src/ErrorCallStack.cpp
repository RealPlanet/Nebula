#include <sstream>
#include <format>
#include <fstream>
#include <string>

#include "ErrorCallStack.h"
#include "Utility.h"
#include "DebugServer.h"

using namespace nebula::shared;

static inline std::string get_line_at(std::ifstream& stream, size_t line, std::string& prev, std::string& next) {
	while (line-- > 1) {
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

ErrorCallStackLine::ErrorCallStackLine(const std::string& scriptSource, const std::string& funcName, size_t instNumber, const std::string& scriptText)
	: m_ScriptSource{ scriptSource }, m_FunctionName{ funcName }, m_InstructionNumber{ instNumber }, m_ScriptLine{ scriptText }
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

		std::string labelCountPrefix = std::format("+ @{}", line.GetInstructionNumber());
		if (i == m_Lines.size() - 1)
		{
			// Root of call stack
			ss << rootPrefix << labelCountPrefix << "::" << line.GetText() << "" << "\n";
			continue;
		}

		strLine = std::format("{}::{}\n", labelCountPrefix, line.GetText());
		strLine.insert(0, rootPrefixCount, ' ');
		ss << strLine;
	}
	ss << "~~~~~~ End of Error ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n";

	return ss.str();
}

bool ErrorCallStack::GetErrorCallStackLineFromDbgFile(const ErrorCallStackLine& line, std::string* outTextLine) const
{
	if (DebugServer::Instance() == nullptr)
	{
		return false;
	}

	ScriptDebugInformation* debugInformation = DebugServer::Instance()->GetDebugInformationForScript(line.GetScriptSource());
	if (debugInformation == nullptr)
	{
		return false;
	}

	FunctionDebugInformation* functionInformation = debugInformation->GetFunctionInformation(line.GetFunctionName());
	if (functionInformation == nullptr)
	{
		return false;
	}

	size_t instructionOpcode = line.GetInstructionNumber();
	size_t lineInfo = functionInformation->GetLineFromOpcode(instructionOpcode);
	if (lineInfo == FunctionDebugInformation::NoLineInfo)
	{
		return false;
	}

	std::stringstream ss;
	ss << std::format("At line {} in function '{}' of script '{}': \n", lineInfo, line.GetFunctionName(), debugInformation->GetOriginalFileName());

	std::ifstream f(debugInformation->GetFullPath());
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
