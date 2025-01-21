#pragma once

#include "CallStackW.h"
#include "LoggerW.h"

namespace nebula
{
    class Interpreter;
}

namespace Nebula::Interop
{
    public enum class InterpreterState
    {
        Abort,
        Running,
        Paused,
        Exited,
    };

    public ref class InterpreterW
    {
    public:
        InterpreterW(LoggerW^ logger);
        !InterpreterW();
        virtual ~InterpreterW();

        bool AddScripts(System::Collections::Generic::ICollection<System::String^>^ scriptPaths);

        void Step();

        void Init(bool startPaused);
        void Run();
        void Pause();
        void Stop();
        void Reset();

        CallStackW^ GetStackFrameOf(int threadId);
        size_t GetCurrentThreadId();
        array<int>^ GetCurrentOpcodeIndexForAllThreads();
        int GetCurrentOpcodeIndexForThread(int threadId);
        int AnyFrameJustStarted(System::String^ _namespace, System::String^ funcName);
        int AnyFrameAt(System::String^ _namespace, System::String^ funcName, int opcode);
    public:
        property int ThreadCount
        {
            int get();
        }

        property InterpreterState State
        {
            InterpreterState get();
        }

    private:
        nebula::Interpreter* _virtualMachine;
        LoggerW^ _logger;
    };
}

