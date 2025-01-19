#include "InterpreterW.h"

#include "Interpreter.h"
#include "Script.h"
#include "InteropUtility.h"

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
    for each (System::String^ sPath in scriptPaths)
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
    const nebula::CallStack* callStack = &_virtualMachine->GetThreadMap().At(threadId);
    CallStackW^ cStack = gcnew CallStackW(callStack);
    return cStack;
}

int InterpreterW::ThreadCount::get()
{
    return (int)_virtualMachine->GetThreadMap().Count();
}

InterpreterState InterpreterW::State::get()
{
    return (InterpreterState)_virtualMachine->GetState();
}
