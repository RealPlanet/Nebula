#include "InterpreterW.h"

#include "Interpreter.h"
#include "Script.h"
#include "InteropUtility.h"
#include "DebuggerDefinitions.h"

#include <vector>
#include <memory>

using namespace Nebula::Interop;

#pragma region Ctor
InterpreterW::InterpreterW(LoggerW^ logger)
{
    _virtualMachine = new nebula::Interpreter();
    _logger = logger;

    if (_logger == nullptr)
    {
        throw gcnew System::ArgumentNullException("logger");
    }
}

InterpreterW::~InterpreterW()
{
    InterpreterW::!InterpreterW();
}

InterpreterW::!InterpreterW()
{
    delete _virtualMachine;
}
#pragma endregion



bool InterpreterW::AddScripts(System::Collections::Generic::ICollection<System::String^>^ scriptPaths)
{
    for each(System::String ^ sPath in scriptPaths)
    {
        std::string nativePath;
        MarshalString(sPath, nativePath);
        nebula::ScriptLoadResult scriptLoadResult = nebula::Script::FromFile(nativePath);

        size_t errCount = scriptLoadResult.ParsingReport.Errors().size();
        if (errCount)
        {
            System::String^ scriptName = System::IO::Path::GetFileNameWithoutExtension(sPath);
            _logger->LogError(System::String::Format("Could not load script '{0}' with '{1}' errors", scriptName, errCount));
            return false;
        }

        bool scriptAddedToVm = _virtualMachine->AddScript(scriptLoadResult.Script);

        if (!scriptAddedToVm)
        {
            System::String^ scriptName = System::IO::Path::GetFileNameWithoutExtension(sPath);
            _logger->LogError(System::String::Format("Could not add script '{0}' to native interpreter", scriptName));
            return false;
        }
    }

    _logger->LogInformation(System::String::Format("Loaded '{0}' script into virtual machine", scriptPaths->Count));
    return true;
}

void InterpreterW::Init(bool startPaused)
{
    _virtualMachine->Init(startPaused);
}

void InterpreterW::Run()
{
    _virtualMachine->Run();
}

void InterpreterW::Pause()
{
    _virtualMachine->Pause();
}

void InterpreterW::Stop()
{
    _virtualMachine->Stop();
}

void InterpreterW::Reset()
{
    _virtualMachine->Reset();
}

void InterpreterW::Step()
{
    _virtualMachine->Step();
}

CallStackW^ InterpreterW::GetStackFrameOf(int threadId)
{
    if (threadId < 0 || threadId >= _virtualMachine->GetThreadMap().Count())
        return nullptr;

    const nebula::CallStack* callStack = &_virtualMachine->GetThreadMap().At(threadId);
    CallStackW^ cStack = gcnew CallStackW(callStack);
    return cStack;
}

size_t Nebula::Interop::InterpreterW::GetCurrentThreadId()
{
    return _virtualMachine->GetCurrentThreadId();
}

array<int>^ InterpreterW::GetNextOpcodeIndexForAllThreads()
{
    const nebula::ThreadMap& threads = _virtualMachine->GetThreadMap();
    array<int>^ result = gcnew array<int>((int)threads.Count());

    for (int i = 0; i < threads.Count(); i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
        {
            result[i] = -1;
            continue;
        }

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        result[i] = frame->NextInstructionIndex();
    }

    return result;
}

int Nebula::Interop::InterpreterW::GetCurrentOpcodeIndexForThread(int threadId)
{
    const nebula::ThreadMap& threads = _virtualMachine->GetThreadMap();

    if (threadId < 0 || threadId >= threads.Count())
        return -1;

    const nebula::CallStack& stack = threads.At(threadId);
    if (stack.size() == 0)
    {
        return -1;
    }

    const nebula::Frame* frame = stack.at(stack.size() - 1);
    size_t index = frame->NextInstructionIndex();
    return (int)index;
}

int InterpreterW::AnyFrameJustStarted(System::String^ _namespace, System::String^ funcName)
{
    const nebula::ThreadMap& threads = _virtualMachine->GetThreadMap();

    for (int i = 0; i < threads.Count(); i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
            continue;

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        // We need JUST started
        if (frame->NextInstructionIndex() != 0)
            continue;

        const nebula::Function* func = frame->GetFunction();
        System::String^ frameNamespace = gcnew System::String(func->Namespace().data());
        System::String^ frameFuncName = gcnew System::String(func->Name().data());
        if (frameNamespace == _namespace && frameFuncName == funcName)
            return i;
    }

    return -1;
}

int InterpreterW::AnyFrameAt(System::String^ _namespace, System::String^ funcName, int opcode)
{
    const nebula::ThreadMap& threads = _virtualMachine->GetThreadMap();

    for (int i = 0; i < threads.Count(); i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
            continue;

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        if (frame->NextInstructionIndex() != opcode)
            continue;

        const nebula::Function* func = frame->GetFunction();
        System::String^ frameNamespace = gcnew System::String(func->Namespace().data());
        System::String^ frameFuncName = gcnew System::String(func->Name().data());
        if (frameNamespace == _namespace && frameFuncName == funcName)
            return i;
    }
    return -1;
}

int InterpreterW::ThreadCount::get()
{
    return (int)_virtualMachine->GetThreadMap().Count();
}

InterpreterState InterpreterW::State::get()
{
    return (InterpreterState)_virtualMachine->GetState();
}
