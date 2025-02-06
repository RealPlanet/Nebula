#pragma once

#include "FrameMemory.h"
#include "Types.h"

namespace Nebula::Interop
{
    public ref class VariableW
    {
    public:
        VariableW(nebula::FrameVariable* ptrNative);

        bool Set(System::String^ obj);

    public:
        property Nebula::CodeEmitter::Types::TypeIdentifier Type
        {
            Nebula::CodeEmitter::Types::TypeIdentifier get();
        }

        property System::Object^ Value
        {
            System::Object^ get();
        }

    private:
        nebula::FrameVariable* m_pNative;
    };
}

