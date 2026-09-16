#include "DebugFile.h"
#include "DebugServer.h"
#include "DefaultDebugServer.h"
#include "Script.h"

#include <cassert>
#include <filesystem>
#include <string>
#include <unordered_map>
#include <utility>

#include "json.h"
#include "json_serialization.h"
// Despite compiler message we NEED this header for the implementation of the JSerializer classes
#include "SymbolsSerializer.h" // VCIC-Excluded 

using namespace nebula;
using namespace nebula::debugger;

void DefaultDebugServer::UnregisterScript(const Script* script)
{
	assert(script);
	if (!script)
	{
		return;
	}

	m_debugSymbols.erase(script->Namespace());
	m_loadedScripts.erase(script->Namespace());
}

void DefaultDebugServer::RegisterScript(const Script* script)
{
	assert(script);
	if (!script)
	{
		return;
	}

	m_loadedScripts.insert_or_assign(script->Namespace(), script);
}

void DefaultDebugServer::UnloadAll()
{
	m_debugSymbols.clear();
	m_loadedScripts.clear();
}

DebugFilePtr DefaultDebugServer::LoadScriptFileFromDisk(const std::string& namespace_)
{
	auto it = m_loadedScripts.find(namespace_);
	if (it == m_loadedScripts.end())
	{
		return nullptr;
	}

	assert(it->second);

	std::filesystem::path path = { it->second->GetSourcePath() };
	path = path.replace_extension(NEBULA_DEBUG_SYMBOL_EXTENSION);

	// Must be on disk file that we can access
	// If the source path is on network this should still work...
	if (!std::filesystem::exists(path))
	{
		return nullptr;
	}

	DebugFilePtr filePtr{ nullptr };
	try
	{
		// Must be a json file
		auto data = strata::json::json_parser::from_file(path);
		auto insertIt = m_debugSymbols.insert(
			std::make_pair(namespace_, strata::json::serialization::Deserialize<symbols::DebugFile>(data)));

		if (!insertIt.second)
		{
			// TODO report?
			return nullptr;
		}

		filePtr = &(insertIt.first)->second;
	}
	catch (...)
	{
		// TODO :: Report maybe? Malformed json...
		return nullptr;
	}

	assert(filePtr);

	// The debug symbols contain the md5 hash of the compiled script.
	// If they differ the files are not compatible
	auto& md5Debug = filePtr->md5Hash;
	// TODO Until MD5 code we always return true here
	auto& scriptMd5 = filePtr->md5Hash;

	if (md5Debug != scriptMd5)
	{
		m_debugSymbols.erase(namespace_);
		return nullptr;
	}

	return filePtr;
}

DebugFilePtr DefaultDebugServer::GetScript(const std::string& namespace_)
{
	auto it = m_debugSymbols.find(namespace_);
	if (it != m_debugSymbols.end())
	{
		return &it->second;
	}

	return LoadScriptFileFromDisk(namespace_);
}

DebugBundleDefinitionPtr DefaultDebugServer::GetBundle(const std::string& namespace_, const std::string& type)
{
	auto scriptInfo = GetScript(namespace_);
	if (!scriptInfo)
	{
		return nullptr;
	}

	auto it = scriptInfo->bundles.find(type);
	if (it == scriptInfo->bundles.end())
	{
		return nullptr;
	}

	return &it->second;
}

DebugFunctionPtr DefaultDebugServer::GetFunction(const std::string& namespace_, const std::string& functionName)
{
	auto scriptInfo = GetScript(namespace_);
	if (!scriptInfo)
	{
		return nullptr;
	}

	auto it = scriptInfo->functions.find(functionName);
	if (it == scriptInfo->functions.end())
	{
		return nullptr;
	}

	return &it->second;
}
