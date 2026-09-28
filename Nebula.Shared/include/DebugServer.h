#pragma once

#ifndef _H_NEBULA_DEBUG_SERVER_
#define _H_NEBULA_DEBUG_SERVER_

#include <string>

#include "DebugSymbols.h"

namespace nebula
{
	class Script;

	namespace debugger
	{
		constexpr const char* NEBULA_DEBUG_SYMBOL_EXTENSION = ".ndbg";

		class DebugServer
		{
		private:
			static DebugServer* m_pInstance;

		public:
			virtual ~DebugServer() = default;

			static DebugServer* Instance();
			static void RegisterDebugServer(DebugServer* Instance);

			// A script is being unloaded so the debug server should be able
			// to release any cached resource
			virtual void UnregisterScript(const Script* script) = 0;

			// Register this script with the debug server for future
			// debug queries, ideally the debug data is not loaded until requested
			virtual void RegisterScript(const Script* script) = 0;

			virtual symbols::DebugSymbols* GetDebugSymbols(const std::string& namespace_) = 0;
		};
	} // namespace debugger
} // namespace nebula

#endif // !_H_NEBULA_DEBUG_SERVER_
