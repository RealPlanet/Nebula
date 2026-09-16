#pragma once

#ifndef _H_EXECUTOR_DBG_SERVER_
#define _H_EXECUTOR_DBG_SERVER_

#include "DefaultDebugServer.h"
#include "DebugController.h"
#include "DebugControllerListener.h"
#include "DebugState.h"
#include "DAPServer.h"
#include "DAPTypes.h"

#include <memory>
#include <string>

namespace nebula
{
	class Interpreter;

	namespace debugger
	{
		using SourceCache = std::unordered_map<std::string /* Namespace */, ::dap::Source >;

		class ExecutorDebugServer
			: public DefaultDebugServer, public dap::DAPListener, public DebugControllerListener
		{
			// Standard debug server stuff
		public:
			explicit ExecutorDebugServer(nebula::Interpreter* virtualMachine);
			~ExecutorDebugServer();

			virtual void UnregisterScript(const Script* script) override;
			virtual DebugFilePtr GetScript(const std::string& namespace_) override;

			virtual void UnloadAll() override;

			// Debugging stuff
		public:
			bool IsDebugging() const;
			const symbols::DebugFunction* GetFunctionAtLine(const std::string& namespace_, size_t line);

		private:
			void AttachDebugger();
			// Clean up debugger logic and returns to predebugging execution
			void DetachDebugger(bool resumeInterpreter);
			void CacheAlreadyLoadedSources();
			void NotifyInterpreterStopped(ThreadId threadId, const dap::StoppedEvent::Reason& reason);

			std::tuple<bool, size_t, std::string, size_t> IsLineDebuggable(const std::string& namespace_, size_t line);

		private:
			bool m_stepOnEntry{ false };
			// We use an additional flag because we want to do some more work after detaching the actual debugger
			bool m_isDebugging{ false };
			nebula::Interpreter* m_pInterpreter;
			DebugStartRequestType m_startRequestType{ DebugStartRequestType::Unkown };

			std::unique_ptr<dap::DAPServer> m_dapServer;
			std::unique_ptr<DebugController> m_debugController;
			std::unique_ptr<DebugState> m_debugState;

			bool m_dapLineStartsAtOne{ false };
			bool m_dapColumnStartsAtOne{ false };

			size_t NormalizeLineNumber(size_t line) const;
			size_t DeNormalizeLineNumber(size_t line) const;

		public:
			// Inherited via DAPListener
			void OnInvalidMessageReceived(const std::string& message) override;
			bool OnInitializeRequest(const dap::InitializeRequest& request, dap::InitializeResponse& response) override;
			bool OnLaunchRequest(const dap::LaunchRequest& request, dap::LaunchResponse& response) override;
			bool OnAttachRequest(const dap::AttachRequest& request, dap::AttachResponse& response) override;
			bool OnLoadedSourcesRequest(const dap::LoadedSourcesRequest& request, dap::LoadedSourcesResponse& response) override;
			bool OnSourceRequest(const dap::SourceRequest& request, dap::SourceResponse& response) override;
			bool OnTerminateRequest(const dap::TerminateRequest& request, dap::TerminateResponse& response) override;
			bool OnDisconnectRequest(const dap::DisconnectRequest& request, dap::DisconnectResponse& response) override;
			bool OnSetExceptionBreakpointsRequest(const dap::SetExceptionBreakpointsRequest& request, dap::SetExceptionBreakpointsResponse& response) override;
			bool OnSetFunctionBreakpointsRequest(const dap::SetFunctionBreakpointsRequest& request, dap::SetFunctionBreakpointsResponse& response) override;
			bool OnSetBreakpointsRequest(const dap::SetBreakpointsRequest& request, dap::SetBreakpointsResponse& response) override;
			bool OnConfigurationDoneRequest(const dap::ConfigurationDoneRequest& request, dap::ConfigurationDoneResponse& response) override;
			bool OnSetVariableRequest(const dap::SetVariableRequest& request, dap::SetVariableResponse& response) override;
			bool OnThreadsRequest(const dap::ThreadsRequest& request, dap::ThreadsResponse& response) override;
			bool OnStackTraceRequest(const dap::StackTraceRequest& request, dap::StackTraceResponse& response) override;
			bool OnScopesRequest(const dap::ScopesRequest& request, dap::ScopesResponse& response) override;
			bool OnVariablesRequest(const dap::VariablesRequest& request, dap::VariablesResponse& response) override;
			bool OnPauseRequest(const dap::PauseRequest& request, dap::PauseResponse& response) override;
			bool OnNextRequest(const dap::NextRequest& request, dap::NextResponse& response) override;
			bool OnStepInRequest(const dap::StepInRequest& request, dap::StepInResponse& response) override;
			bool OnContinueRequest(const dap::ContinueRequest& request, dap::ContinueResponse& response) override;

			// Inherited via DebugControllerListener
			void OnInterpreterPaused(ThreadId threadId, PauseReason reason) override;
			void OnInterpreterResumed(ThreadId threadId) override;
			void OnInterpreterTerminated() override;
			void OnBreakpointHit(DebugBreakpoint& breakpoint) override;
			void OnOutput(const std::string& output) override;
			void OnInterpreterFatalError(const nebula::shared::ErrorCallStack* error) override;
		};
	}
}

#endif // !_H_EXECUTOR_DBG_SERVER_
