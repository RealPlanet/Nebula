#pragma once

#include "VariableW.h"

#include "Frame.h"

namespace Nebula::Interop
{
    public ref class StackFrameW
    {
    public:
        StackFrameW(nebula::Frame* nativePtr);

        VariableW^ GetLocalVariableAt(int index);
        VariableW^ GetParameterVariableAt(int index);

    public:
        property System::String^ FunctionName
        {
            System::String^ get();
        }

        property System::String^ Namespace
        {
            System::String^ get();
        }

        property int NextInstructionIndex
        {
            int get();
        }

        property int InstructionCount
        {
            int get();
        }

        property int LocalCount
        {
            int get();
        }

        property int ParameterCount
        {
            int get();
        }

    private:
        nebula::Frame* m_pNative;
    };
}
