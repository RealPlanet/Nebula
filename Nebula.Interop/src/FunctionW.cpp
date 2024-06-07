#include "Function.h"
#include "FunctionW.h"

using namespace Nebula::Interpreter;

FunctionW::FunctionW(const nebula::Function* nativeDefinition)
    : m_NativePtr{ nativeDefinition }
{
    m_Name = gcnew System::String(nativeDefinition->Name().data());
    m_ReturnType = (Nebula::CodeEmitter::Types::TypeIdentifier)nativeDefinition->ReturnType();

    for (nebula::VMAttribute attr : nativeDefinition->Attributes())
    {
        m_Attributes->Add(gcnew FunctionAttributeW(attr));
    }

    for (const nebula::DataStackVariantIndex& parameterType : nativeDefinition->Parameters())
    {
        m_Parameters->Add(gcnew FunctionParameterW(parameterType));
    }

    // Load all instructions
    for (const nebula::FunctionInstruction& inst : nativeDefinition->Instructions())
    {
        m_Instructions->Add(gcnew InstructionW(inst.first));
    }
}

Nebula::Interpreter::InstructionW::InstructionW(nebula::VMInstruction opcode)
    : m_Opcode{ (Nebula::CodeEmitter::InstructionOpcode)opcode }
{

}

Nebula::Interpreter::FunctionParameterW::FunctionParameterW(nebula::DataStackVariantIndex type)
    : m_Type{ (Nebula::CodeEmitter::Types::TypeIdentifier)type }
{
}

Nebula::Interpreter::FunctionAttributeW::FunctionAttributeW(System::String^ name)
    : m_RawName{ name }
{
    
    if (!System::Enum::TryParse(m_RawName, m_NativeAttribute))
    {
        m_NativeAttribute = Nebula::CodeEmitter::NativeAttribute::Uknown;
    }
}

Nebula::Interpreter::FunctionAttributeW::FunctionAttributeW(nebula::VMAttribute nativeAttribute)
{
    if (nativeAttribute == nebula::VMAttribute::uknown)
    {
        throw gcnew System::NotSupportedException("Attribute of type unknown is not supported, use from string constructor");
    }

    m_NativeAttribute = (Nebula::CodeEmitter::NativeAttribute)nativeAttribute;
    m_RawName = m_NativeAttribute.ToString();
}
