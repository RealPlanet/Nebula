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
    return m_pNative->CurrentInstructionIndex();
}

int StackFrameW::LocalCount::get()
{
    return m_pNative->Memory().LocalCount();
}

int StackFrameW::ParameterCount::get()
{
    return m_pNative->Memory().ParamCount();
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
