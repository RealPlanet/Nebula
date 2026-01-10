#include <string>
#include <filesystem>
#include <fstream>
#include <map>

#include "DebugServer.h"
#include "DefaultDebugServer.h"
#include "Script.h"

#include "json.hpp"

using json = nlohmann::json;

using namespace nebula;

void DefaultDebugServer::NotifyScriptUnloaded(const Script* script)
{
	auto it = m_DebugCache.find(script);
	if (it != m_DebugCache.end())
	{
		delete it->second;
		m_DebugCache.erase(it);
	}
}

ScriptDebugInformation* DefaultDebugServer::GetDebugInformationForScript(const Script* script)
{
	auto it = m_DebugCache.find(script);
	if (it != m_DebugCache.end())
	{
		return it->second;
	}

	const std::string& sourceFilePath = script->GetSourcePath();

	// Must be on disk file that we can access
	if (!std::filesystem::exists(sourceFilePath)) {
		return nullptr;
	}

	std::filesystem::path path = { sourceFilePath };
	path = path.replace_extension(".ndbg");

	// Must be on disk file that we can access for debug information
	if (!std::filesystem::exists(path)) {
		return nullptr;
	}

	std::ifstream f(path);
	json data = json::parse(f);
	f.close();

	ScriptDebugInformation* dbgInfo = new ScriptDebugInformation();
	m_DebugCache[script] = dbgInfo;

	dbgInfo->SetOriginalFileName(data["OriginalFileName"]);
	auto& functions = data["Functions"];
	for (auto& funcData : functions) {
		std::string funcName = funcData["Name"];
		std::map<std::string, size_t> lineIndicies = funcData["LineStartingOpcodeIndex"];
		size_t lineNumber = funcData["LineNumber"];
		size_t endLineNumber = funcData["EndLineNumber"];
		size_t instructionCount = funcData["InstructionCount"];

		dbgInfo->AddFunctionInformation(FunctionDebugInformation{ funcName, lineNumber, endLineNumber, instructionCount, lineIndicies });
	}
	data.clear();
	return dbgInfo;
}