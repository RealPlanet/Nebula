#pragma once

#include "Function.h"

#include "Types.h"
#include "InstructionOpcode.h"
#include "NativeAttribute.h"


namespace Nebula::Interop
{
    public ref class FunctionParameterW
    {
    public:
        FunctionParameterW(nebula::DataStackVariantIndex);
        property  Nebula::CodeEmitter::Types::TypeIdentifier Type
        {
            Nebula::CodeEmitter::Types::TypeIdentifier get() { return m_Type; }
        }
    private:
        Nebula::CodeEmitter::Types::TypeIdentifier m_Type;
    };

    public ref class FunctionAttributeW
    {
    public:
        FunctionAttributeW(System::String^ name);
        FunctionAttributeW(nebula::VMAttribute nativeAttribute);

        property System::String^ RawName { System::String^ get() { return m_RawName; } }
        property Nebula::CodeEmitter::NativeAttribute NativeAttribute { Nebula::CodeEmitter::NativeAttribute get() { return m_NativeAttribute; } }
    private:
        System::String^ m_RawName;
        Nebula::CodeEmitter::NativeAttribute m_NativeAttribute;
    };

    public ref class InstructionW
    {
    public:
        InstructionW(nebula::VMInstruction);

        property Nebula::CodeEmitter::InstructionOpcode Opcode
        {
            Nebula::CodeEmitter::InstructionOpcode get() { return m_Opcode; }
        }
    private:
        Nebula::CodeEmitter::InstructionOpcode m_Opcode;
    };

    public ref class FunctionW
    {
    public:
        property Nebula::CodeEmitter::Types::TypeIdentifier ReturnType { Nebula::CodeEmitter::Types::TypeIdentifier get() { return m_ReturnType; } }

        property System::String^ Name { System::String^ get() { return m_Name; } }

        property System::Collections::Generic::List< FunctionAttributeW^>^ Attributes
        {
            System::Collections::Generic::List< FunctionAttributeW^>^ get() { return m_Attributes; }
        }

        property System::Collections::Generic::List< FunctionParameterW^>^ Parameters
        {
            System::Collections::Generic::List< FunctionParameterW^>^ get() { return m_Parameters; }
        }

        property System::Collections::Generic::List< InstructionW^>^ Instructions
        {
            System::Collections::Generic::List< InstructionW^>^ get() { return m_Instructions; }
        }

        FunctionW(const nebula::Function* nativeDefinition);
        virtual System::String^ ToString() override { return Name; }
    private:
        const nebula::Function* m_NativePtr{ nullptr };
        System::String^ m_Name;
        Nebula::CodeEmitter::Types::TypeIdentifier m_ReturnType;
        System::Collections::Generic::List< FunctionAttributeW^>^ m_Attributes = gcnew System::Collections::Generic::List< FunctionAttributeW^>();
        System::Collections::Generic::List< FunctionParameterW^>^ m_Parameters = gcnew System::Collections::Generic::List< FunctionParameterW^>();
        System::Collections::Generic::List< InstructionW^>^ m_Instructions = gcnew System::Collections::Generic::List< InstructionW^>();
    };
}



