#include <string>
#include <filesystem>
#include <fstream>
#include <map>	
#include <vector>

#include "DebugServer.h"
#include "DefaultDebugServer.h"

#include "json.hpp"

using json = nlohmann::json;
using namespace nebula;

static void from_json(const json& j, LineDebugInformation& p) {
	size_t lineNumber = j.at("LineNumber");
	size_t startOpcode = j.at("StartOpcodeOfLine");

	p.SetOpcode(startOpcode);
	p.SetLineNumber(lineNumber);
}

void DefaultDebugServer::NotifyScriptUnloaded(const std::string& scriptSource)
{
	auto it = m_DebugCache.find(scriptSource);
	if (it != m_DebugCache.end())
	{
		delete it->second;
		m_DebugCache.erase(it);
	}
}

ScriptDebugInformation* DefaultDebugServer::GetDebugInformationForScript(const std::string& scriptSource)
{
	auto it = m_DebugCache.find(scriptSource);
	if (it != m_DebugCache.end())
	{
		return it->second;
	}

	// Must be on disk file that we can access
	if (!std::filesystem::exists(scriptSource)) {
		return nullptr;
	}

	std::filesystem::path path = { scriptSource };
	path = path.replace_extension(".ndbg");

	// Must be on disk file that we can access for debug information
	if (!std::filesystem::exists(path)) {
		return nullptr;
	}

	std::ifstream f(path);
	json data = json::parse(f);
	f.close();

	ScriptDebugInformation* dbgInfo = new ScriptDebugInformation();
	m_DebugCache[scriptSource] = dbgInfo;

	dbgInfo->SetOriginalFileName(data["OriginalFileName"]);
	dbgInfo->SetFullPath(data["OriginalFileFullName"]);
	auto& functions = data["Functions"];
	for (auto& funcData : functions) {
		std::string funcName = funcData["Name"];
		std::vector<json> jsonLines = funcData["Lines"];
		std::vector<LineDebugInformation> lines = {};

		for (auto& j : jsonLines) {
			LineDebugInformation line;
			from_json(j, line);
			lines.push_back(line);
		}

		size_t lineNumber = funcData["LineNumber"];
		size_t endLineNumber = funcData["EndLineNumber"];
		size_t instructionCount = funcData["InstructionCount"];

		dbgInfo->AddFunctionInformation(FunctionDebugInformation{ funcName, lineNumber, endLineNumber, instructionCount, lines });
	}
	data.clear();
	return dbgInfo;
}

