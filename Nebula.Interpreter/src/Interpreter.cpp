#include "Interpreter.h"

#include "Frame.h"
#include "Utility.h"
#include "InterpreterStandardOutput.h"

#include <chrono>
#include <cassert>
#include <format>

using namespace nebula;

Interpreter::Interpreter()
    : m_LastErrorCallstack{ nullptr }, m_pStandardOutput{ nullptr }, m_Memory{ this }
{
    SetStandardOutput(new InterpreterStandardOutput());
}

Interpreter::~Interpreter()
{
    // Run gc to avoid leaks
    m_Memory.Collect(true);
    assert(m_Memory.Empty());

    SetState(State::Exited);
    m_Threads.Clear();
    m_Scripts.clear();
    m_NativeFunctions.clear();
    ClearStandardOutput();
    delete m_LastErrorCallstack;
}

Interpreter::State Interpreter::Init(bool startPaused /*= false*/)
{
    SetState(startPaused ? State::Paused : State::Running);
    m_LastSchedulingUpdate = std::chrono::high_resolution_clock::now();
    SwapExecutingThread();
    return GetState();
}

Interpreter::State Interpreter::Run()
{
    while (m_CurrentState == State::Running || m_CurrentState == State::Paused)
    {
        if (m_CurrentState == State::Paused)
        {
            m_IsVMRunning.wait(true);
            continue;
        }

        // Exit once we can no longer step
        if (!Step())
        {
            break;
        }
    }

    return GetState();
}

Interpreter::State Interpreter::Pause()
{
    SetState(State::Paused);
    return GetState();
}

Interpreter::State Interpreter::Stop()
{
    SetState(State::Exited);
    return GetState();
}

void Interpreter::Reset()
{
    m_Threads.Clear();
    m_CurrentThreadIndex = 0;
    delete m_LastErrorCallstack;

    m_NativeFunctions.clear();
    m_Scripts.clear();

    m_LastSchedulingUpdate = std::chrono::high_resolution_clock::now();

    // Ready to go!
    m_StartedOnce = false;
}

bool Interpreter::BindNativeFunction(const std::string& name, const NativeFunctionCallback callback)
{
    if (m_NativeFunctions.find(name) != m_NativeFunctions.end()) {
        return false;
    }

    m_NativeFunctions.insert(std::make_pair(name, callback));
    return true;
}

bool Interpreter::AddScript(std::shared_ptr<Script> script)
{
    if (script->Namespace() == "") {
        return false;
    }

    if (m_Scripts.find(script->Namespace()) != m_Scripts.end()) {
        return false;
    }

    m_Scripts.insert(std::make_pair(script->Namespace(), script));

    for (auto& kvp : script->Functions()) {
        if (std::find(kvp.second.Attributes().begin(), kvp.second.Attributes().end(), VMAttribute::AutoExec) == kvp.second.Attributes().end()) {
            continue;
        }

        CreateFrameOnStack(&kvp.second, true);
    }

    return true;
}

bool Interpreter::SetStandardOutput(IStreamWrapper* stream)
{
    if (stream == nullptr)
        return false;

    delete m_pStandardOutput;
    m_pStandardOutput = stream;
    return true;
}

bool nebula::Interpreter::SetExitCallback(InterpreterExitCallbackPtr callbackPtr)
{
    m_fExitCallback = callbackPtr;
    return true;
}

bool Interpreter::ClearStandardOutput() {
    delete m_pStandardOutput;
    m_pStandardOutput = nullptr;
    return true;
}

bool Interpreter::Step()
{
    if (CheckAndSetExitState())
        return false; // Early exit

    Frame* currentFrame = GetCurrentCallstack()->back();
    Frame::Status frameStatus = currentFrame->Tick(this);
    switch (frameStatus)
    {
    case Frame::Status::FatalError:
    {
        BuildErrorStack(currentFrame);
        SetState(State::Abort);
        break;
    }
    case Frame::Status::Finished:

        delete currentFrame;
        GetCurrentCallstack()->pop_back();
        break;
    }

    if (GetCurrentCallstack()->empty())
    {
        m_Threads.RemoveCallstack(m_CurrentThreadIndex);
        SwapExecutingThread();
    }
    else if (ShouldScheduleNewFrame())
    {
        SwapExecutingThread();
    }

    CheckAndSetExitState();
    return true;
}

bool Interpreter::CheckAndSetExitState()
{
    if (GetState() == State::Abort)
    {
        m_Threads.Clear();
        return true;
    }

    if (!m_Threads.HasCallStacks())
    {
        SetState(State::Exited);
        if (m_fExitCallback != nullptr)
        {
            m_fExitCallback();
        }

        return true;
    }

    return false;
}

void Interpreter::SetState(State state)
{
    if (m_CurrentState == state)
        return;

    m_CurrentState = state;
    if (state == State::Running)
    {
        m_IsVMRunning.test_and_set();
        return;
    }

    if (state == State::Paused)
    {
        //m_IsVMRunning.clear();
        return;
    }

    if (state == State::Abort || state == State::Exited)
    {
        m_IsVMRunning.clear();
        return;
    }
}

bool Interpreter::ShouldScheduleNewFrame()
{
    if (m_Threads.Count() == 1 || m_Threads.Count() == 0)
    {
        return false;
    }

    auto newUpdate = std::chrono::high_resolution_clock::now();
    long long passedMs = std::chrono::duration_cast<std::chrono::milliseconds>(newUpdate - m_LastSchedulingUpdate).count();

    if (passedMs >= m_MaxExecutionTime)
    {
        m_LastSchedulingUpdate = newUpdate;
        return true;
    }

    return false;
}

void Interpreter::SwapExecutingThread()
{
    if (m_Threads.Count() == 0 || m_Threads.Count() == 1)
    {
        m_CurrentThreadIndex = 0;
        return;
    }

    // Calculate the new index
    {
        m_CurrentThreadIndex++;
        if (m_CurrentThreadIndex >= m_Threads.Count())
        {
            m_CurrentThreadIndex = 0;
        }
    }
}

void Interpreter::CreateFrameOnStack(const Function* f, bool separateThread)
{
    assert(f);

    Frame* parent{ nullptr };

    CallStack* cStack = GetCurrentCallstack();
    if (cStack && cStack->size() > 0)
    {
        parent = cStack->back();
    }

    Frame* newFrame = new Frame(parent, f, separateThread);
    if (separateThread)
    {
        CallStack* newThreadCallstack = m_Threads.CreateNewThread();
        newThreadCallstack->reserve(2);
        newThreadCallstack->push_back(newFrame);
        return;
    }

    assert(cStack);
    cStack->emplace_back(newFrame);
}

void Interpreter::BuildErrorStack(Frame* fatalFrame)
{
    assert(fatalFrame);
    if (!fatalFrame)
        return;

    delete m_LastErrorCallstack;
    m_LastErrorCallstack = new nebula::shared::ErrorCallStack();

    Frame* current = fatalFrame;

    InstructionErrorCode eCode = fatalFrame->GetLastError();
    std::string readableError = InstructionErrorCodeToString(eCode);
    std::string readableErrorFormatted = std::format("Fatal error ({}) : {}", (int)eCode, readableError);
    m_LastErrorCallstack->SetExplanation(eCode, readableErrorFormatted);

    //size_t spaceCount = 0;
    while (current)
    {
        const std::string& ns = current->GetFunction()->Namespace();
        const std::string& funcName = current->GetFunction()->Name();
        std::string callstackLine = std::format("{}::{}(...) -> ", ns, funcName) + BuildGuiltyInstructionLineForCallStack(current);
        size_t labelIndex = current->NextInstructionIndex() - 1;

        //callstackLine.insert(0, spaceCount++, ' ');
        const std::string& sourcePath = current->GetFunction()->GetScript()->GetSourcePath();
        m_LastErrorCallstack->Append(nebula::shared::ErrorCallStackLine{ sourcePath , funcName, labelIndex, callstackLine});
        current = current->Parent();
    }
}

std::string Interpreter::BuildGuiltyInstructionLineForCallStack(Frame* f)
{
    size_t labelIndex = f->NextInstructionIndex() - 1;

    auto& currInstruction = f->GetFunction()->Instructions()[labelIndex];
    VMInstruction currentOpcode = currInstruction.first;

    std::string line = itos(currentOpcode);
    line += " ";
    for (auto& arg : currInstruction.second)
    {
        line += (ToString(arg) + " ");
    }

    line = trim(line);
    return line;
}

const Function* Interpreter::GetFunction(const std::string& scriptNamespace, const std::string& funcName) const
{
    auto scriptIt = m_Scripts.find(scriptNamespace);
    if (scriptIt == m_Scripts.end())
        return nullptr;

    const std::shared_ptr<Script>& script = scriptIt->second;
    const FunctionMap& funcMap = script->Functions();
    auto funcIt = funcMap.find(funcName);
    if (funcIt == funcMap.end())
        return nullptr;

    return &funcIt->second;
}

const BundleDefinition* Interpreter::GetBundleDefinition(const std::string& scriptNamespace, const std::string& bundleName) const
{
    auto scriptIt = m_Scripts.find(scriptNamespace);
    if (scriptIt == m_Scripts.end())
        return nullptr;

    auto& allBundles = scriptIt->second->Bundles();
    auto bundleIt = allBundles.find(bundleName);
    if (bundleIt == allBundles.end())
        return nullptr;

    return &bundleIt->second;
}

const NativeFunctionCallback* Interpreter::GetNativeFunction(const std::string& funcName) const
{
    const auto& nativeFunc = m_NativeFunctions.find(funcName);
    if (nativeFunc == m_NativeFunctions.end())
        return nullptr;

    const NativeFunctionCallback& func = nativeFunc->second;
    return &func;
}

CallStack* Interpreter::GetCurrentCallstack()
{
    if (!m_Threads.HasCallStacks())
        return nullptr;

    return &m_Threads.At(m_CurrentThreadIndex);
}
