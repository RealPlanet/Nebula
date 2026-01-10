#include <fstream>
#include <sstream>
#include <format>
#include <ostream>

#include "Script.h"
#include "interfaces\IScriptParser.h"
#include "LiteralScriptParser.h"
#include "Function.h"
#include "DebugServer.h"

using namespace nebula;

ScriptLoadResult nebula::Script::FromFile(const std::string& filePath)
{
	std::ifstream fs(filePath);

	if (!fs.is_open() || fs.bad())
	{
		ScriptLoadResult errResult;
		errResult.ParsingReport.ReportError(std::format("Could not open file at: {}", filePath));
		return errResult;
	}

	std::stringstream buffer;
	buffer << fs.rdbuf();
	return std::move(FromMemory(buffer.str(), filePath));
}

ScriptLoadResult nebula::Script::FromMemory(const std::string_view& data, const std::string& sourcePath)
{
	std::unique_ptr<IScriptParser> parser = std::make_unique<parsing::ParserDebug>();

	ScriptLoadResult result;
	result.Script = parser->ParseScript(data);
	result.Script->m_SourcePath = sourcePath;
	result.ParsingReport = parser->GetLastParsingReport();
	return result;
}

nebula::Script::~Script()
{
	if (DebugServer::Instance())
	{
		DebugServer::Instance()->NotifyScriptUnloaded(m_SourcePath);
	}
}

Script::Script()
{
}

ScriptBuilder::ScriptBuilder()
{
	m_InternalScript = new Script();
}

ScriptBuilder::~ScriptBuilder()
{
	delete m_InternalScript;
}

Script* ScriptBuilder::Finalize() noexcept
{
	Script* result = m_InternalScript;
	m_InternalScript = nullptr;
	return result;
}

bool ScriptBuilder::HasNamespace() const
{
	return m_InternalScript->Namespace() != "";
}

bool ScriptBuilder::SetNamespace(const std::string& ns)
{
	if (HasNamespace())
		return false;

	m_InternalScript->m_Namespace = ns;
	return true;
}

bool ScriptBuilder::AddFunction(Function&& func)
{
	if (m_InternalScript->m_Functions.find(func.Name()) != m_InternalScript->m_Functions.end())
	{
		return false;
	}

	m_InternalScript->m_Functions.emplace(func.Name(), func);
	return true;
}

bool ScriptBuilder::AddBundle(BundleDefinition&& bundle)
{
	if (m_InternalScript->m_Bundles.find(bundle.Name()) != m_InternalScript->m_Bundles.end())
	{
		return false;
	}

	m_InternalScript->m_Bundles.emplace(bundle.Name(), bundle);
	return true;
}
