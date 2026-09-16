#pragma once

#ifndef _H_DEBUG_SERVER_
#define _H_DEBUG_SERVER_

#include <string>

#include "DebugBundleDefinition.h"
#include "DebugFile.h"
#include "DebugFunction.h"
#include "DebugVariable.h"

namespace nebula
{
	class Script;

	namespace debugger
	{
		using DebugFilePtr = const symbols::DebugFile*;
		using DebugBundleDefinitionPtr = const symbols::DebugBundleDefinition*;
		using DebugFunctionPtr = const symbols::DebugFunction*;
		using DebugVariablePtr = const symbols::DebugVariable*;

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

			virtual DebugFilePtr GetScript(const std::string& namespace_) = 0;
			virtual DebugBundleDefinitionPtr GetBundle(const std::string& namespace_, const std::string& type) = 0;
			virtual DebugFunctionPtr GetFunction(const std::string& namespace_, const std::string& type) = 0;
		};
	} // namespace debugger
} // namespace nebula

#endif // !_H_DEBUG_SERVER_
