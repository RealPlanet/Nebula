#include "StackFrameW.h"
#include "Function.h"

using namespace Nebula::Interop;

StackFrameW::StackFrameW(const nebula::Frame* nativePtr)
    : m_pNative{ nativePtr }
{

}

System::String^ StackFrameW::FunctionName::get()
{
    const nebula::Function* func = m_pNative->GetFunction();

    return gcnew System::String(func->Name().data());
}

System::String^ StackFrameW::Namespace::get()
{
    const nebula::Function* func = m_pNative->GetFunction();
    return gcnew System::String(func->Namespace().data());
}


int StackFrameW::CurrentInstructionIndex::get()
{
    size_t index = m_pNative->CurrentInstructionIndex();
    if (index == nebula::Frame::frame_not_started)
        return -1;
    return static_cast<int>(index);
}

int StackFrameW::LocalCount::get()
{
    size_t count = m_pNative->Memory().LocalCount();
    return static_cast<int>(count);
}

int StackFrameW::ParameterCount::get()
{
    size_t count = m_pNative->Memory().ParamCount();
    return static_cast<int>(count);
}


VariableW^ Nebula::Interop::StackFrameW::GetLocalVariableAt(int index)
{
    if (index < 0 || index >= this->LocalCount)
        return nullptr;

    const nebula::FrameVariable& v = m_pNative->Memory().LocalAt(index);
    return gcnew VariableW(&v);
}

VariableW^ Nebula::Interop::StackFrameW::GetParameterVariableAt(int index)
{
    if (index < 0 || index >= this->ParameterCount)
        return nullptr;

    const nebula::FrameVariable& v = m_pNative->Memory().ParamAt(index);
    return gcnew VariableW(&v);
}

int Nebula::Interop::StackFrameW::InstructionCount::get()
{
    size_t count = m_pNative->GetFunction()->Instructions().size();
    return static_cast<int>(count);
}
