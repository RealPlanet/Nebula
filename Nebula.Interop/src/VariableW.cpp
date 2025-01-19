#include "VariableW.h"

using namespace Nebula::Interop;
using namespace Nebula::CodeEmitter::Types;

VariableW::VariableW(const nebula::FrameVariable* ptrNative)
    : m_pNative{ ptrNative }
{
}

TypeIdentifier Nebula::Interop::VariableW::Type::get()
{
    return (TypeIdentifier)m_pNative->Type();
}

System::Object^ Nebula::Interop::VariableW::Value::get()
{
    if (m_pNative->Value().index() != m_pNative->Type())
        return nullptr; //In some init cases type is set but no value is present. For example when breaking as soon as we enter in a function scope


    switch (Type)
    {
    case TypeIdentifier::String:
        return gcnew System::String(m_pNative->AsString().data());
    case TypeIdentifier::Int32:
        return m_pNative->AsInt32();
    default:
        throw gcnew System::Exception("Unsupported value type");
    }
}
