#pragma once

#include <map>

#include "DebugServer.h"

namespace nebula {
	class Script;

	class DefaultDebugServer
		: public DebugServer {
	private:
		std::map<std::string, ScriptDebugInformation*> m_DebugCache;

	public:
		void NotifyScriptUnloaded(const std::string& scriptSource);
		ScriptDebugInformation* GetDebugInformationForScript(const std::string& scriptSource);
	};


}