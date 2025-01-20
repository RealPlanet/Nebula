#include "CallStackW.h"
#include "CallStack.h"
#include "Frame.h"

#include <stdint.h>

using namespace Nebula::Interop;

CallStackW::CallStackW(const nebula::CallStack* nativePtr)
    : m_pNative{ nativePtr }
{
}

System::Collections::Generic::List<StackFrameW^>^ Nebula::Interop::CallStackW::ToList()
{
    System::Collections::Generic::List<StackFrameW^>^ frames = gcnew System::Collections::Generic::List<StackFrameW^>();
    for (int i = (int)m_pNative->size() - 1; i >= 0; i--)
    {
        frames->Add(gcnew StackFrameW(m_pNative->at(i)));
    }

    return frames;
}

StackFrameW^ Nebula::Interop::CallStackW::LastFrame::get()
{
    return gcnew StackFrameW(m_pNative->at(m_pNative->size() - 1));
}

int Nebula::Interop::CallStackW::FrameCount::get()
{
    return (int)m_pNative->size();
}
