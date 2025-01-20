#pragma once

#include "StackFrameW.h"

#include "CallStack.h"

namespace Nebula::Interop
{
    public ref class CallStackW
    {
    public:
        CallStackW(const nebula::CallStack* nativePtr);

        System::Collections::Generic::List<StackFrameW^>^ ToList();

        property StackFrameW^ LastFrame
        {
            StackFrameW ^ get();
        }

        property int FrameCount
        {
            int get();
        }

    private:
        const nebula::CallStack* m_pNative;
    };
}

