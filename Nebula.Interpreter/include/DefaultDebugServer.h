#pragma once

#ifndef _H_DEFAULT_DEBUG_SERVER_
#define _H_DEFAULT_DEBUG_SERVER_

#include <string>
#include <unordered_map>

#include "DebugServer.h"
#include "DebugFile.h"

namespace nebula
{
	class Script;

	namespace debugger
	{
		class DefaultDebugServer
			: public DebugServer
		{
		public:
			// Inherited via DebugServer
			virtual void UnregisterScript(const Script* script) override;
			virtual void RegisterScript(const Script* script) override;

			virtual DebugFilePtr GetScript(const std::string& namespace_) override;
			virtual DebugBundleDefinitionPtr GetBundle(const std::string& namespace_, const std::string& type) override;
			virtual DebugFunctionPtr GetFunction(const std::string& namespace_, const std::string& functionName) override;

			virtual void UnloadAll();

		private:
			std::unordered_map<std::string /* namespace*/, symbols::DebugFile> m_debugSymbols;
			std::unordered_map<std::string /* namespace*/, const Script*> m_loadedScripts;

			DebugFilePtr LoadScriptFileFromDisk(const std::string& namespace_);
		};
	} // namespace debugger
} // namespace nebula

#endif // !_H_DEFAULT_DEBUG_SERVER_
