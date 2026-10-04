#include "Frame.h"
#include "Interpreter.h"
#include "InstructionRegistry.h"

using namespace nebula;

Frame::Frame(Frame* parent, const Function* f, bool discardParent)
	: m_ParentFrame{ parent },
	m_Memory{ f->Parameters().size(), f->Locals().size() },
	m_FunctionDefinition{ f },
	m_NextInstructionIndex{ 0 },
	m_Scheduler{ this }
{
	const VariableList& params = f->Parameters();
	DataStack& dataStack = parent->Stack();
	long paramCount = (long)params.size();

	// Stack values are in the opposite order!
	for (long i = paramCount - 1; i >= 0; i--)
	{
		Value& param = m_Memory.ParamAt(i);
		param.SetValue(dataStack.Peek());
		dataStack.Pop();
	}

	if (discardParent)
	{
		m_ParentFrame = nullptr;
	}
	else
	{
		m_ParentFrame->m_ChildFrame = this;
	}
}

Frame::Frame(Frame&& f) noexcept
	: m_FunctionDefinition{ f.m_FunctionDefinition },
	m_ParentFrame{ f.m_ParentFrame },
	m_ChildFrame{ f.m_ChildFrame },
	m_NextInstructionIndex{ f.m_NextInstructionIndex },
	m_Scheduler{ std::move(f.m_Scheduler) },
	m_Memory{ std::move(f.m_Memory) },
	m_Stack{ std::move(f.m_Stack) },
	m_LastErrorCode{ f.m_LastErrorCode }
{
	f.m_FunctionDefinition = nullptr;
	f.m_ParentFrame = nullptr;
	f.m_ChildFrame = nullptr;
	f.m_NextInstructionIndex = 0;
	f.m_LastErrorCode = InstructionErrorCode::None;
}

Frame::Status Frame::Tick(Interpreter* interpreter)
{
	// If this frame has received a kill notification we need to halt all child frames
	if (m_Scheduler.HasBeenKilled())
	{
		if (m_ChildFrame != nullptr)
			m_ChildFrame->Kill();

		if (m_ParentFrame != nullptr)
		{
			// TODO :: Figure if this is what we actually want! Ideally we'd return null and let the user handle it
			if (m_FunctionDefinition->ReturnType() != DataStackVariantIndex::_TypeVoid)
			{
				m_LastErrorCode = InstructionErrorCode::EndonKilledUnthreadedFunctionWithReturnValue;
				return Status::FatalError;
			}
		}

		return Frame::Status::Finished;
	}

	if (m_Scheduler.IsSleeping())
		return Frame::Status::Paused;

	const FunctionBody& insts = m_FunctionDefinition->Instructions();
	const FunctionInstruction& theInstruction = insts[m_NextInstructionIndex++];
	const VMInstruction& opCode = theInstruction.first;
	const InstructionArguments& args = theInstruction.second;

	if (m_NextInstructionIndex >= insts.size())
	{
		if (opCode != VMInstruction::Ret && opCode != VMInstruction::Br)
			return Status::FatalError;
	}

	InstructionErrorCode executionError = ExecuteInstruction(opCode, interpreter, this, args);

	if (executionError != InstructionErrorCode::None)
	{
		m_LastErrorCode = executionError;
		return Status::FatalError;
	}

	if (opCode == VMInstruction::Ret)
	{
		return Status::Finished;
	}

	return Status::Running;
}

Frame::Status nebula::Frame::RunToCompletion(Interpreter* interpreter)
{
	Status status = Tick(interpreter);
	while (status == Status::Running)
	{
		status = Tick(interpreter);
	}

	return status;
}

const std::string& Frame::Namespace()  const
{
	return m_FunctionDefinition->Namespace();
}

const std::string& Frame::FunctionName() const
{
	return m_FunctionDefinition->Name();
}

void Frame::SetScheduledSleep(const size_t& amount)
{
	m_Scheduler.Sleep(amount);
}

void Frame::SetNextInstruction(size_t label)
{
	[[unlikely]]
	if (label >= m_FunctionDefinition->Instructions().size())
		throw std::exception("Label is out of bounds!");

	m_NextInstructionIndex = label;
}

void Frame::WaitForNotification(GCUser* notifier, const std::string& notification)
{
	m_Scheduler.WaitForNotification(notifier, notification);
}

void Frame::EndOnNotification(GCUser* notifier, const std::string& notification)
{
	m_Scheduler.EndOnNotification(notifier, notification);
}

void nebula::Frame::Kill()
{
	m_Scheduler.Kill();
	if (m_ChildFrame != nullptr)
	{
		m_ChildFrame->Kill();
	}
}

