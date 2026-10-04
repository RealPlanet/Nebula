#pragma once

#ifndef _H_NEBULA_DEFAULT_DEBUG_SERVER_
#define _H_NEBULA_DEFAULT_DEBUG_SERVER_

#include <string>
#include <unordered_map>

#include "DebugServer.h"
#include "DebugSymbols.h"

namespace nebula
{
	class Script;

	namespace debugger
	{
		class DefaultDebugServer : public DebugServer
		{
		public:
			// Inherited via DebugServer
			virtual void UnregisterScript(const Script* script) override;
			virtual void RegisterScript(const Script* script) override;

			virtual symbols::DebugSymbols* GetDebugSymbols(const std::string& namespace_) override;
			virtual void UnloadAll();

		private:
			std::unordered_map<std::string /* namespace*/, symbols::DebugSymbols> m_debugSymbols;
			std::unordered_map<std::string /* namespace*/, const Script*> m_loadedScripts;

			symbols::DebugSymbols* LoadScriptFileFromDisk(const std::string& namespace_);
		};
	} // namespace debugger
} // namespace nebula

#endif // !_H_NEBULA_DEFAULT_DEBUG_SERVER_
