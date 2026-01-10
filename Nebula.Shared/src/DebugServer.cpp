#include "DebugServer.h"

using namespace nebula;

DebugServer* DebugServer::m_pInstance{ nullptr };

FunctionDebugInformation::FunctionDebugInformation(const std::string& funcName, size_t lineNumber, size_t endLineNumber, size_t instructionCount, std::vector<LineDebugInformation>& lines)
	: m_Name{ funcName }, m_LineNumber{ lineNumber }, m_EndLineNumber{ endLineNumber }, m_InstructionCount{ instructionCount }, m_Lines{lines}
{

}

size_t FunctionDebugInformation::GetLineFromOpcode(size_t opcode) const
{
	if (opcode < 0)
	{
		return NoLineInfo;
	}

	size_t lastPossibleLine = 0;
	for (auto& kvp : m_Lines)
	{
		size_t lineNumber = kvp.GetLineNumber() + 1;
		size_t startingOpcodeOfLine = kvp.GetOpcode();

		if (startingOpcodeOfLine == opcode) {
			return lineNumber + 1;
		}

		if (startingOpcodeOfLine > opcode) {
			return lastPossibleLine + 1;
		}

		lastPossibleLine = lineNumber;
	}

	return NoLineInfo;
}

FunctionDebugInformation* ScriptDebugInformation::GetFunctionInformation(const std::string& funcName)
{
	auto it = m_Functions.find(funcName);
	if (it != m_Functions.end())
	{
		return &it->second;
	}

	return nullptr;
}


DebugServer* DebugServer::Instance()
{
	return m_pInstance;
}

void DebugServer::RegisterDebugServer(DebugServer* instance)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}

	m_pInstance = instance;
}
