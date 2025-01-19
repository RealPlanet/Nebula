#pragma once

#include "Types.h"

namespace nebula
{
    class BundleDefinition; // Native FW
}

namespace Nebula::Interop
{
    public ref class BundleFieldW
    {
    public:
        BundleFieldW(System::String^ name, Nebula::CodeEmitter::Types::TypeIdentifier type);
        property System::String^ Name { System::String^ get() { return m_Name; }}
        property Nebula::CodeEmitter::Types::TypeIdentifier Type { Nebula::CodeEmitter::Types::TypeIdentifier get() { return m_Type; }}

        virtual System::String^ ToString() override { return m_Name + " " + m_Type.ToString(); }
    private:
        System::String^ m_Name;
        Nebula::CodeEmitter::Types::TypeIdentifier m_Type;
    };

    public ref class BundleW
    {
    public:
        BundleW(const nebula::BundleDefinition* nativeDefinition);
        property System::String^ Name { System::String^ get() { return m_Name; } }
        property System::Collections::Generic::List< BundleFieldW^>^ Fields
        {
            System::Collections::Generic::List< BundleFieldW^>^ get() { return m_Fields; }
        }

        virtual System::String^ ToString() override { return Name; }
    private:
        const nebula::BundleDefinition* m_NativePtr{ nullptr };
        System::String^ m_Name;
        System::Collections::Generic::List< BundleFieldW^>^ m_Fields = gcnew System::Collections::Generic::List< BundleFieldW^>();
    };
}
