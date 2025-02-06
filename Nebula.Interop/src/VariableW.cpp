#include "VariableW.h"
#include "InteropUtility.h"
using namespace Nebula::Interop;
using namespace Nebula::CodeEmitter::Types;

VariableW::VariableW(nebula::FrameVariable* ptrNative)
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

bool Nebula::Interop::VariableW::Set(System::String^ obj)
{
    System::Type^ t = obj->GetType();
    if (Type == TypeIdentifier::String)
    {
        std::string nativeStr;
        Nebula::Interop::MarshalString((System::String^)obj, nativeStr);
        nebula::DataStackVariant variant{ nativeStr };
        return m_pNative->SetValue(variant);
    }

    if (Type == TypeIdentifier::Int32)
    {
        int value;
        if (!System::Int32::TryParse(obj, value))
        {
            return false;
        }

        nebula::DataStackVariant variant{ value };
        return m_pNative->SetValue(variant); 
    }

    return false;
}
