#pragma once

#ifndef _H_NEBULA_DEBUG_CONTROLLER_
#define _H_NEBULA_DEBUG_CONTROLLER_

#include "BlockingQueue.h"
#include "DebugState.h"
#include "DebuggerEntities.h"
#include "BreakpointManager.h"
#include "ExceptionCallstack.h"

#include <atomic>
#include <thread>

namespace nebula
{
	class Interpreter;
	class Frame;
}

namespace nebula::debugger
{
	enum class PauseReason
	{
		Entry,
		Step,
		Stopped,
	};

	class DebugControllerListener
	{
	public:
		virtual void OnInterpreterPaused(ThreadId threadId, PauseReason reason) = 0;
		virtual void OnInterpreterResumed(ThreadId threadId) = 0;
		virtual void OnInterpreterTerminated() = 0;
		virtual void OnBreakpointHit(Breakpoint& breakpoint) = 0;
		virtual void OnInterpreterFatalError(const nebula::shared::ExceptionCallstack* error) = 0;
		virtual void OnOutput(const std::string& output) = 0;
	};

	class DebugController
	{
	public:
		DebugController(Interpreter* interpreter, DebugControllerListener* listener, DebugState* debugState);
		~DebugController();

		void StartDebugger();
		void StopDebugger(bool resumeInterpreter);

		void Step(ThreadId thread);
		void StepIn(ThreadId thread);
		void Continue(ThreadId thread);

		bool IsPaused() const;
		bool IsDebugging() const;

		// Requests that the current operation to stops and waits for it
		void StopCurrentOperation();

		const Interpreter& GetInterpreter() const { return *m_interpreter; }
		BreakpointManager& GetBreakpointManager() { return m_breakpointManager; }

	private:
		enum class ContinueResult
		{
			Unkown,
			Error,
			Hitbreakpoint,
			Done
		};

		struct DebugEventInfo
		{
			enum class Event
			{
				Unkown,
				Step,
				StepIn,
				Continue,
			};

			Event type;
			ThreadId threadId;
		};

		void StopOperationThread();
		void OperationThread();

		void ProcessStep(ThreadId thread);
		void ProcessStepIn(ThreadId thread);
		void ProcessContinue(ThreadId thread);
		bool Step();
		void CheckInterpreterExited();

		void StepLine(ThreadId threadId);
		void StepInto(ThreadId threadId);
		void StepOverFunctionCall(ThreadId threadId, nebula::Frame* ourFrame, size_t callstackIndex);

		bool HandleStepOfExitingFunction(ThreadId id, size_t nextOpcode, size_t instructionCount);
		bool AtEndOfFunction(size_t nextOpcode, size_t instructionCount);
		bool AnyBreakpointHit();
		ThreadId AnyFrameJustStarted(const std::string& _namespace, const std::string& funcName);
		ThreadId AnyFrameAboutToBeAt(const std::string& _namespace, const std::string& funcName, size_t opcode);

		const symbols::FunctionInformation* GetFunctionInformation(const std::string& _namespace, const std::string& funcName);

		Interpreter* m_interpreter;
		DebugControllerListener* m_listener;
		DebugState* m_debugState;

		std::thread m_operationThread;
		collections::BlockingQueue<DebugEventInfo> m_eventQueue;
		std::atomic_flag m_processingEvent = ATOMIC_FLAG_INIT;
		BreakpointManager m_breakpointManager;

		bool m_isDebugging;
		bool m_runOperationThread;
		bool m_stopCurrentOperation;
	};
}

#endif // !_H_NEBULA_DEBUG_CONTROLLER_
