#include "DebugController.h"
#include "Interpreter.h"
#include "DebugServer.h"
#include "ExecutorDebugServer.h"

#include <cassert>

using namespace nebula;
using namespace nebula::debugger;

DebugController::DebugController(Interpreter* interpreter, DebugControllerListener* listener)
	: m_interpreter(interpreter), m_listener{ listener }
{
	assert(interpreter);
	assert(listener);

	m_stopCurrentOperation = false;
	m_runOperationThread = false;
	m_isDebugging = false;

	// Exit is handled in the Step wrapper method
	//m_interpreter->SetExitCallback([this]() {
	//	assert(m_listener);
	//		m_listener->OnInterpreterTerminated();
	//	});
}

DebugController::~DebugController()
{
	StopDebugger(true);
}

void DebugController::StartDebugger()
{
	if (m_isDebugging)
	{
		return;
	}

	m_interpreter->Pause();
	m_isDebugging = true;
	m_runOperationThread = true;
	m_operationThread = std::thread(&DebugController::OperationThread, this);
}

void DebugController::StopDebugger(bool resumeInterpreter)
{
	if (!m_isDebugging)
	{
		return;
	}

	StopCurrentOperation();
	StopOperationThread();

	m_eventQueue.RequestShutdown();
	if (m_operationThread.joinable())
	{
		m_operationThread.join();
	}

	m_isDebugging = false;
	if (resumeInterpreter)
	{
		m_interpreter->Resume();
	}
}

void DebugController::Step(ThreadId thread)
{
	m_eventQueue.Push({
		.type = DebugEventInfo::Event::Step,
		.threadId = thread,
		});
}

void DebugController::StepIn(ThreadId thread)
{
	m_eventQueue.Push({
		.type = DebugEventInfo::Event::StepIn,
		.threadId = thread,
		});
}

void DebugController::Continue(ThreadId thread)
{
	m_eventQueue.Push({
		.type = DebugEventInfo::Event::Continue,
		.threadId = thread,
		});
}

bool DebugController::IsPaused() const
{
	return !m_processingEvent.test();
}

bool DebugController::IsDebugging() const
{
	return m_isDebugging;
}

void DebugController::StopCurrentOperation()
{
	m_stopCurrentOperation = true;
	m_processingEvent.wait(true);
}

void DebugController::StopOperationThread()
{
	StopCurrentOperation();
	m_runOperationThread = false;
	m_eventQueue.RequestShutdown();
	m_operationThread.join();
}

void DebugController::OperationThread()
{
	DebugEventInfo currentEvent;
	while (m_runOperationThread)
	{
		assert(m_listener);
		if (!m_eventQueue.Pop(currentEvent))
		{
			// Queue has been closed
			break;
		}

		auto interpreterState = m_interpreter->GetState();
		if (interpreterState != Interpreter::Paused)
		{
			// TODO Notify?
			assert(false);
			break;
		}

		m_processingEvent.test_and_set();
		m_processingEvent.notify_all();

		try
		{
			switch (currentEvent.type)
			{
			case DebugEventInfo::Event::Step:
			{
				ProcessStep(currentEvent.threadId);
				break;
			}
			case DebugEventInfo::Event::StepIn:
			{
				ProcessStepIn(currentEvent.threadId);
				break;
			}
			case DebugEventInfo::Event::Continue:
			{
				ProcessContinue(currentEvent.threadId);
				break;
			}
			case DebugEventInfo::Event::Unkown:
			default:
			{
				// TODO Report
				break;
			}
			}
		}
		catch (...)
		{
			assert(false);
			// TODO Handle / Report
		}

		m_processingEvent.clear();
		m_processingEvent.notify_all();
	}
}

void DebugController::ProcessStep(ThreadId thread)
{
	m_listener->OnInterpreterResumed(thread);
	StepLine(thread);

	if (m_interpreter->GetState() == Interpreter::State::Paused)
	{
		m_listener->OnInterpreterPaused(thread, PauseReason::Step);
	}
}

void DebugController::ProcessStepIn(ThreadId thread)
{
	m_listener->OnInterpreterResumed(thread);

	if (m_interpreter->GetState() == Interpreter::State::Paused)
	{
		m_listener->OnInterpreterPaused(thread, PauseReason::Step);
	}
}

void DebugController::ProcessContinue(ThreadId thread)
{
	m_listener->OnInterpreterResumed(thread);
	while (!m_stopCurrentOperation)
	{
		if (!Step())
		{
			return;
		}
	}

	if (m_interpreter->GetState() == Interpreter::State::Paused)
	{
		m_listener->OnInterpreterPaused(m_interpreter->GetCurrentThreadId(), PauseReason::Stopped);
	}
}

bool DebugController::Step()
{
	if (!m_interpreter->Step())
	{
		m_stopCurrentOperation = true;
		m_runOperationThread = false;

		auto errorCallstack = m_interpreter->GetFatalErrorCallstack();
		if (errorCallstack)
		{
			m_listener->OnInterpreterFatalError(errorCallstack);
		}

		// Step returns if the vm exited too
		m_listener->OnInterpreterTerminated();
		return false;
	}

	if (AnyBreakpointHit())
	{
		return false;
	}

	return true;
}

void DebugController::StepLine(ThreadId threadId)
{
	auto thread = &m_interpreter->GetThread(threadId);
	if (thread->size() == 0)
	{
		// TODO :: Report
		return;
	}

	Frame* lastFrameOfThread = thread->at(thread->size() - 1);
	auto callstackCount = thread->size();
	auto nextOpcode = lastFrameOfThread->NextInstructionIndex();
	auto dbgInfo = static_cast<ExecutorDebugServer*>(DebugServer::Instance())->GetFunction(lastFrameOfThread->Namespace(), lastFrameOfThread->FunctionName());
	if (!dbgInfo)
	{
		Step();
		return;
	}

	size_t nextLine = dbgInfo->GetLineFromOpcode(nextOpcode);
	size_t lastStepLine = nextLine;

	if (HandleStepOfExitingFunction(threadId, nextOpcode, dbgInfo->instructionCount))
	{
		return;
	}

	do
	{
		if (!Step())
		{
			return;
		}

		nextOpcode = lastFrameOfThread->NextInstructionIndex();
		if (HandleStepOfExitingFunction(threadId, nextOpcode, dbgInfo->instructionCount))
		{
			// Function of thread exited we stop
			return;
		}

		thread = &m_interpreter->GetThread(threadId);
		assert(m_interpreter->GetThread(threadId).size() != 0);

		// We called a function, need to keep stepping until we get back here
		StepOverFunctionCall(threadId, lastFrameOfThread, callstackCount);

		lastStepLine = dbgInfo->GetLineFromOpcode(lastFrameOfThread->NextInstructionIndex());
		// because we can have statements/expressions that span multiple lines, as soon as we pass the expected line we stop
	} while (lastStepLine == nextLine && !m_stopCurrentOperation);
}

void DebugController::StepOverFunctionCall(ThreadId threadId, Frame* ourFrame, size_t callstackIndex)
{
	assert(m_interpreter->GetThread(threadId).size() != 0);

	auto thread = &m_interpreter->GetThread(threadId);
	Frame* newLastFrame = thread->at(thread->size() - 1);
	while (newLastFrame->FunctionName() != ourFrame->FunctionName() ||
		newLastFrame->Namespace() != ourFrame->Namespace() ||
		thread->size() != callstackIndex /* Even if the function is the same as our frame we must make sure it is not a recursive call */)
	{
		if (m_stopCurrentOperation)
		{
			return;
		}

		if (!Step())
		{
			return;
		}

		newLastFrame = thread->at(thread->size() - 1);
	}

	// new last frame should be equal to the original frame
	assert(ourFrame == newLastFrame);
}

bool DebugController::HandleStepOfExitingFunction(ThreadId id, size_t nextOpcode, size_t instructionCount)
{
	if (!AtEndOfFunction(nextOpcode, instructionCount))
	{
		return false;
	}

	// Return to our thread for stepping
	while (m_interpreter->GetCurrentThreadId() != id)
	{
		if (!Step())
		{
			return true;
		}
	}

	// We step once, this will cause the function to return and the callstack to be deleted if it's empty
	Step();
	return true;
}

bool DebugController::AtEndOfFunction(size_t nextOpcode, size_t instructionCount)
{
	if (nextOpcode != instructionCount - 1)
	{
		return false;
	}

	return true;
}

bool DebugController::AnyBreakpointHit()
{
	auto scopelock = m_breakpointManager.Lock();

	for (auto& bp : m_breakpointManager.GetFunctionBreakpoints())
	{
		auto& ns = bp.GetNamespace();
		auto& funcName = bp.GetFunctionName();
		ThreadId id = AnyFrameJustStarted(ns, funcName);

		if (id == NO_THREAD_ID)
		{
			continue;
		}

		::nebula::debugger::DebugBreakpoint hitBreakpoint;
		hitBreakpoint.isFunctionBreakpoint = true;
		hitBreakpoint.threadId = id;
		m_listener->OnBreakpointHit(hitBreakpoint);
		return true;
	}

	for (auto& kvp : m_breakpointManager.GetBreakpoints())
	{
		auto& bps = kvp.second;
		for (auto& bp : bps)
		{
			auto& ns = bp.GetNamespace();
			auto& funcName = bp.GetFunctionName();
			auto opcodeIndex = bp.GetOpcodeIndex();

			ThreadId id = AnyFrameAboutToBeAt(ns, funcName, opcodeIndex);
			if (id == NO_THREAD_ID)
			{
				continue;
			}


			::nebula::debugger::DebugBreakpoint hitBreakpoint;
			hitBreakpoint.isFunctionBreakpoint = false;
			hitBreakpoint.threadId = id;
			m_listener->OnBreakpointHit(hitBreakpoint);
			return true;
		}
	}

	return false;
}

ThreadId DebugController::AnyFrameJustStarted(const std::string& _namespace, const std::string& funcName)
{
	const nebula::ThreadMap& threads = m_interpreter->GetThreads();
	for (ThreadId i = 0; i < threads.Count(); i++)
	{
		const nebula::CallStack& stack = threads.At(i);
		if (stack.size() == 0)
		{
			continue;
		}

		const nebula::Frame* frame = stack.at(stack.size() - 1);
		// We need JUST started
		if (frame->NextInstructionIndex() != 0)
		{
			continue;
		}

		const nebula::Function* func = frame->GetFunction();
		if (func->Namespace() != _namespace)
		{
			continue;
		}

		if (func->Name() != funcName)
		{
			continue;
		}


		return i;
	}

	return NO_THREAD_ID;
}

ThreadId DebugController::AnyFrameAboutToBeAt(const std::string & _namespace, const std::string & funcName, size_t opcode)
{
	const nebula::ThreadMap& threads = m_interpreter->GetThreads();
	for (int i = 0; i < threads.Count(); i++)
	{
		const nebula::CallStack& stack = threads.At(i);
		if (stack.size() == 0)
			continue;

		const nebula::Frame* frame = stack.at(stack.size() - 1);
		if (frame->NextInstructionIndex() != opcode)
		{
			continue;
		}

		const nebula::Function* func = frame->GetFunction();
		if (func->Namespace() != _namespace)
		{
			continue;
		}

		if (func->Name() != funcName)
		{
			continue;
		}

		return i;
	}

	return NO_THREAD_ID;
}

//
//ContinueResult DebugController::StepIn(ThreadId threadId, HitBreakpointInformation& hitBreakpointInfo)
//{
//	const nebula::CallStack& thread = m_interpreter->GetThreads()
//		.At(threadId);
//	if (thread.size() == 0)
//	{
//		// TODO :: Report
//		return ContinueResult::Error;
//	}
//
//	Frame* lastFrame = thread.at(thread.size() - 1);
//	auto nextOpcode = lastFrame->NextInstructionIndex();
//	FunctionDbgData* dbgInfo = DebugServer::Instance()->GetFunctionDbgData(lastFrame->Namespace(), lastFrame->FunctionName());
//	if (!dbgInfo)
//	{
//		m_interpreter->Step();
//		return ContinueResult::Done;
//	}
//
//	size_t lineNumber = dbgInfo->GetLineFromOpcode(nextOpcode);
//	size_t nextLine = lineNumber;
//
//	// TODO need to check for breakpoints in case we hit something on different threads ?
//	do
//	{
//		m_interpreter->Step();
//		Frame* newLastFrame = thread.at(thread.size() - 1);
//		if (lastFrame->FunctionName() != newLastFrame->FunctionName() ||
//			lastFrame->Namespace() != newLastFrame->Namespace())
//		{
//			break;
//		}
//
//		lastFrame = newLastFrame;
//		nextLine = dbgInfo->GetLineFromOpcode(newLastFrame->NextInstructionIndex());
//
//	} while (lineNumber == nextLine && !m_haltStepping);
//
//	return ContinueResult::Done;
//
//}



