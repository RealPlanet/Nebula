#pragma once

#include "FrameMemory.h"
#include "Types.h"

namespace Nebula::Interop
{
    public ref class VariableW
    {
    public:
        VariableW(const nebula::FrameVariable* ptrNative);

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
        const nebula::FrameVariable* m_pNative;
    };
}

