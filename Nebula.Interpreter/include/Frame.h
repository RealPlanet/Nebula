#pragma once

#include "FrameScheduler.h"
#include "FrameMemory.h"
#include "DataStack.h"
#include "Instruction.h"

namespace nebula
{
    class Interpreter;
    class Function;


    // Rapresents a asingular executing function call
    class Frame
    {
    public:
        static constexpr auto frame_not_started{ static_cast<size_t>(-1) };
        enum Status
        {
            FatalError, // Fatal error requires VM to stop
            Running,
            Paused,
            Finished,
        };

    public:
        // if discardParent is true then parent is not saved
        // Async functions receive the parent ptr to fetch the parameters data
        // but musn't store the parent ptr, otherwise they'll try to return values to it
        Frame(Frame* parent, const Function* f, bool discardParent);
        Frame(Frame&& f) = delete;
        Frame(const Frame& f) = delete;
        ~Frame() = default;

        // Execute the next instruction
        Status Tick(Interpreter*);

        inline DataStack& Stack() { return m_Stack; }
        inline FrameMemory& Memory() { return m_Memory; }
        inline Frame* Parent() { return m_ParentFrame; }
        inline const FrameMemory& Memory() const { return m_Memory; };
        inline const Function* GetFunction() const { return m_FunctionDefinition; }
        inline InstructionErrorCode GetLastError() const { return m_LastErrorCode; }
        inline size_t CurrentInstructionIndex() const { return m_NextInstructionIndex - 1; }
        const std::string& Namespace();

    public:
        void SetScheduledSleep(const size_t& amount) { m_Scheduler.Sleep(amount); }
        void SetNextInstruction(size_t index);

    private:
        // The read-only definition of this function
        const Function* m_FunctionDefinition;
        Frame* m_ParentFrame;

        size_t m_NextInstructionIndex;
        FrameScheduler m_Scheduler;
        FrameMemory m_Memory;
        DataStack m_Stack;

        InstructionErrorCode m_LastErrorCode{ InstructionErrorCode::None };
    };
}

