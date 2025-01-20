#pragma once

#include <atomic>
#include <chrono>
#include <functional>

#include "ThreadMap.h"
#include "LanguageTypes.h"

#include "Frame.h"
#include "Script.h"
#include "Instruction.h"
#include "ErrorCallStack.h"

namespace nebula
{
    // Function pointer definition to bind Nebula function calls to C++ calls
    using NativeFunctionCallback = ::std::function<InstructionErrorCode(Interpreter*, Frame*)>;

    // Core of the virtual machine
    class Interpreter
    {
        friend InstructionErrorCode ExecuteInstruction(VMInstruction, Interpreter*, Frame*, const InstructionArguments&);

    public:
        enum State
        {
            Abort,
            Running,
            Paused,
            Exited,
        };

    public:
        Interpreter();
        ~Interpreter();
        Interpreter(const Interpreter&) = delete;
        Interpreter(Interpreter&&) = delete;

    public:
        State GetState() const { return m_CurrentState; }
        State Wait() { m_IsVMRunning.wait(true); return GetState(); }
        State InitAndRun(bool startPaused = false) { Init(startPaused); return Run(); }
        State Init(bool startPaused = false);
        State Run();
        State Pause();
        State Stop();
        void Reset();

        shared::ErrorCallStack* GetFatalErrorCallstack() { return m_LastErrorCallstack; }

        // Native functions are responsible to fetch the data from the parent data stack
        bool BindNativeFunction(const std::string& name, const NativeFunctionCallback callback);
        bool AddScript(std::shared_ptr<Script> script);

        bool Step();
        const ThreadMap& GetThreadMap() { return m_Threads; }
        const size_t GetCurrentThreadId() const { return m_CurrentThreadIndex; }
    private:
        bool CheckAndSetExitState();

        void SetState(State);
        bool ShouldScheduleNewFrame();
        void SwapExecutingThread();
        void CreateFrameOnStack(const Function*, bool);
        void BuildErrorStack(Frame*);
        std::string BuildInstructionLineForCallStack(Frame*);

        const Function* GetFunction(const std::string&, const std::string&) const;
        const BundleDefinition* GetBundleDefinition(const std::string&, const std::string&) const;
        const NativeFunctionCallback* GetNativeFunction(const std::string&) const;

        /// <summary>
        /// Thread map can be re-allocated at any time, do not store a ptr to the current callstack
        /// </summary>
        /// <returns></returns>
        CallStack* GetCurrentCallstack();
    private:
        std::map<const std::string, NativeFunctionCallback> m_NativeFunctions{};
        std::map<const std::string, std::shared_ptr<Script>> m_Scripts{};

        ThreadMap m_Threads;
        size_t m_CurrentThreadIndex{ 0 };

        State m_CurrentState{ State::Paused };
        bool m_StartedOnce{ false };
        std::atomic_flag m_IsVMRunning = ATOMIC_FLAG_INIT;
        shared::ErrorCallStack* m_LastErrorCallstack;

        int m_MaxExecutionTime = 10;// Milliseconds
        std::chrono::steady_clock::time_point m_LastSchedulingUpdate{};
    };
}