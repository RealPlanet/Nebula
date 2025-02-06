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
        FrameVariable& param = m_Memory.ParamAt(i);
        param._type = params[i];
        param._value = dataStack.Peek();
        dataStack.Pop();
    }

    const VariableList& vars = f->Locals();
    for (size_t i = 0; i < vars.size(); i++)
    {
        m_Memory.LocalAt(i)._type = vars[i];
        // Value is set when instructions are executed
    }

    if (discardParent)
        m_ParentFrame = nullptr;
}

Frame::Status Frame::Tick(Interpreter* interpreter)
{
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

const std::string& Frame::Namespace()
{
    return m_FunctionDefinition->Namespace();
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
