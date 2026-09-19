#pragma once

#ifndef _H_DEBUG_CONTROLLER_H
#define _H_DEBUG_CONTROLLER_H

#include "BlockingQueue.h"
#include "BreakpointManager.h"
#include "DebugControllerListener.h"
#include "DebugTypes.h"

#include <atomic>
#include <thread>

namespace nebula
{
	class Interpreter;
	class Frame;
}

namespace nebula::debugger
{
	/// <summary>
	/// Provides debugging functionality for the virtual machine.
	/// </summary>
	class DebugController
	{
	public:
		DebugController(Interpreter* interpreter, DebugControllerListener* listener);
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
		void StopOperationThread();
		void OperationThread();

		void ProcessStep(ThreadId thread);
		void ProcessStepIn(ThreadId thread);
		void ProcessContinue(ThreadId thread);
		bool Step();
		void CheckInterpreterExited();

		void StepLine(ThreadId threadId);
		void StepOverFunctionCall(ThreadId threadId, Frame* ourFrame, size_t callstackIndex);

		bool HandleStepOfExitingFunction(ThreadId id, size_t nextOpcode, size_t instructionCount);
		bool AtEndOfFunction(size_t nextOpcode, size_t instructionCount);
		bool AnyBreakpointHit();
		ThreadId AnyFrameJustStarted(const std::string& _namespace, const std::string& funcName);
		ThreadId AnyFrameAboutToBeAt(const std::string& _namespace, const std::string& funcName, size_t opcode);

		Interpreter* m_interpreter;
		DebugControllerListener* m_listener;

		std::thread m_operationThread;
		collections::BlockingQueue<DebugEventInfo> m_eventQueue;
		std::atomic_flag m_processingEvent = ATOMIC_FLAG_INIT;
		BreakpointManager m_breakpointManager;

		bool m_isDebugging;
		bool m_runOperationThread;
		bool m_stopCurrentOperation;

		//private:
		//	// All these methods must be invoked by the dispatch queue
		//	ContinueResult StepLine(ThreadId threadId, HitBreakpointInformation& hitBreakpointInfo);
		//	ContinueResult StepStatement(ThreadId threadId, HitBreakpointInformation& hitBreakpointInfo);
		//	ContinueResult StepIn(ThreadId threadId, HitBreakpointInformation& hitBreakpointInfo);
		//	

		//	bool SteppingOverLastInstruction(ThreadId threadId, size_t nextOpcode, FunctionDbgData* dbgInfo);
		//private:
		//	// Handles the code execution logic while debugging
		//	
		//	void ProcessContinueEvent(DebugEventInfo& info);
		//	void ProcessStepEvent(DebugEventInfo& info);
		//	void ProcessStepInEvent(DebugEventInfo& info);

		//	bool AnyBreakpointHit(HitBreakpointInformation& hitBreakpoint);
		//	
		//	
	};
}



#endif // !_H_DEBUG_CONTROLLER_H
