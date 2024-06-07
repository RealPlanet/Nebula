#include "BundleW.h"

#include "Bundle.h"

using namespace Nebula::Interpreter;

BundleFieldW::BundleFieldW(System::String^ name, Nebula::CodeEmitter::Types::TypeIdentifier type)
    : m_Name{ name }, m_Type{ type }
{
}

BundleW::BundleW(const nebula::BundleDefinition* nativeDefinition)
    : m_NativePtr{ nativeDefinition }
{
    m_Name = gcnew System::String(nativeDefinition->Name().data());

    auto it = nativeDefinition->Fields().cbegin();
    while (it != nativeDefinition->Fields().cend())
    {
        System::String^ name = gcnew System::String(it->first.c_str());
        Nebula::CodeEmitter::Types::TypeIdentifier type = (Nebula::CodeEmitter::Types::TypeIdentifier)it->second;

        BundleFieldW^ field = gcnew BundleFieldW(name, type);
        m_Fields->Add(field);
        it++;
    }
}


