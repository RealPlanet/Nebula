#include "DebugController.h"
#include "CallStack.h"
#include "Interpreter.h"
#include "DebugServer.h"
#include "ExecutorDebugServer.h"

#include "Frame.h"

#include <cassert>

using namespace nebula;
using namespace nebula::debugger;

DebugController::DebugController(Interpreter* interpreter, DebugControllerListener* listener, DebugState* debugState)
	: m_interpreter(interpreter), m_listener{ listener }, m_debugState{ debugState }
{
	assert(interpreter);
	assert(listener);
	assert(debugState);

	m_stopCurrentOperation = false;
	m_runOperationThread = false;
	m_isDebugging = false;
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

		assert(m_interpreter->GetState() == nebula::Interpreter::State::Paused);
		auto interpreterState = m_interpreter->GetState();
		if (interpreterState != Interpreter::Paused)
		{
			// TODO Notify?
			assert(false);
			break;
		}

		m_processingEvent.test_and_set();
		m_processingEvent.notify_all();
		m_stopCurrentOperation = false;

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

	StepInto(thread);

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
		CheckInterpreterExited();
		return false;
	}

	if (AnyBreakpointHit())
	{
		return false;
	}

	return true;
}

void DebugController::CheckInterpreterExited()
{
	m_stopCurrentOperation = true;
	m_runOperationThread = false;

	auto ExceptionCallstack = m_interpreter->GetFatalExceptionCallstack();
	if (ExceptionCallstack)
	{
		m_listener->OnInterpreterFatalError(ExceptionCallstack);
	}

	// Step returns if the vm exited too
	m_listener->OnInterpreterTerminated();
}

void DebugController::StepLine(ThreadId threadId)
{
	auto thread = &m_interpreter->GetThread(threadId);
	if (thread->size() == 0)
	{
		// TODO :: Report
		return;
	}

	nebula::Frame* lastFrameOfThread = thread->at(thread->size() - 1);

	const symbols::FunctionInformation* functionInformation
		= GetFunctionInformation(lastFrameOfThread->Namespace(), lastFrameOfThread->FunctionName());
	if (functionInformation == nullptr)
	{
		Step();
		return;
	}

	auto nextOpcode = lastFrameOfThread->NextInstructionIndex();
	size_t initialCallstackSize = thread->size();
	size_t nextLine = functionInformation->GetLineFromOpcode(nextOpcode);
	size_t lastStepLine = nextLine;

	if (HandleStepOfExitingFunction(threadId, nextOpcode, functionInformation->instructionCount))
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
		if (HandleStepOfExitingFunction(threadId, nextOpcode, functionInformation->instructionCount))
		{
			// Function of thread exited we stop
			return;
		}

		thread = &m_interpreter->GetThread(threadId);
		assert(m_interpreter->GetThread(threadId).size() != 0);

		// We called a function, need to keep stepping until we get back here
		StepOverFunctionCall(threadId, lastFrameOfThread, initialCallstackSize);

		lastStepLine = functionInformation->GetLineFromOpcode(lastFrameOfThread->NextInstructionIndex());
		// because we can have statements/expressions that span multiple lines, as soon as we pass the expected line we stop
	} while (lastStepLine == nextLine && !m_stopCurrentOperation);
}

void DebugController::StepInto(ThreadId threadId)
{
	auto thread = &m_interpreter->GetThread(threadId);
	if (thread->size() == 0)
	{
		// TODO :: Report
		return;
	}

	nebula::Frame* lastFrameOfThread = thread->at(thread->size() - 1);
	auto nextOpcode = lastFrameOfThread->NextInstructionIndex();

	const symbols::FunctionInformation* functionInformation
		= GetFunctionInformation(lastFrameOfThread->Namespace(), lastFrameOfThread->FunctionName());

	if (functionInformation == nullptr)
	{
		Step();
		return;
	}

	size_t lineNumber = functionInformation->GetLineFromOpcode(nextOpcode);
	size_t nextLine = lineNumber;

	do
	{
		m_interpreter->Step();
		nebula::Frame* newLastFrame = thread->at(thread->size() - 1);
		if (lastFrameOfThread->FunctionName() != newLastFrame->FunctionName() ||
			lastFrameOfThread->Namespace() != newLastFrame->Namespace())
		{
			break;
		}

		lastFrameOfThread = newLastFrame;
		functionInformation = GetFunctionInformation(lastFrameOfThread->Namespace(), lastFrameOfThread->FunctionName());
		if(functionInformation == nullptr)
		{
			break;
		}

		nextLine = functionInformation->GetLineFromOpcode(newLastFrame->NextInstructionIndex());
	} while (lineNumber == nextLine && !m_stopCurrentOperation);
}

void DebugController::StepOverFunctionCall(ThreadId threadId, nebula::Frame* ourFrame, size_t callstackIndex)
{
	assert(m_interpreter->GetThread(threadId).size() != 0);

	auto thread = &m_interpreter->GetThread(threadId);
	nebula::Frame* newLastFrame = thread->at(thread->size() - 1);
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
	auto lock = std::scoped_lock{ m_breakpointManager.GetMutex() };

	for (auto& bp : m_breakpointManager.GetFunctionBreakpoints())
	{
		auto& ns = bp.GetNamespace();
		auto& funcName = bp.GetFunctionName();
		ThreadId id = AnyFrameJustStarted(ns, funcName);
		if (id == INVALID_THREAD_ID)
		{
			continue;
		}

		::nebula::debugger::Breakpoint hitBreakpoint = {};
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
			if (id == INVALID_THREAD_ID)
			{
				continue;
			}

			::nebula::debugger::Breakpoint hitBreakpoint = {};
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

	return INVALID_THREAD_ID;
}

ThreadId DebugController::AnyFrameAboutToBeAt(const std::string& _namespace, const std::string& funcName, size_t opcode)
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

	return INVALID_THREAD_ID;
}

const symbols::FunctionInformation* nebula::debugger::DebugController::GetFunctionInformation(const std::string& _namespace, const std::string& funcName)
{
	const Source* frameSource = m_debugState->GetSource(_namespace);
	if (frameSource == nullptr ||
		frameSource->debugSymbols == nullptr)
	{
		return nullptr;

	}

	const std::string& functionName = funcName;
	const symbols::FunctionInformation* functionInformation = frameSource->debugSymbols->GetFunction(functionName);
	if (functionInformation == nullptr)
	{
		Step();
		return nullptr;
	}

	return functionInformation;
}
