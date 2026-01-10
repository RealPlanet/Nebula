#pragma once

#include <map>

#include "DebugServer.h"

namespace nebula {
	class Script;

	class DefaultDebugServer
		: public DebugServer {
	private:
		std::map<const Script*, ScriptDebugInformation*> m_DebugCache;

	public:
		void NotifyScriptUnloaded(const Script* script);
		ScriptDebugInformation* GetDebugInformationForScript(const Script* script);
	};
}