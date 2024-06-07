#pragma once

#include "BundleW.h"
#include "FunctionW.h"
#include "SharedPtrW.h"

namespace nebula
{
    class Script; // Native FW
}

namespace Nebula::Interpreter
{
    public ref class CompiledScript
    {
    public:
        static bool LoadScriptFromFile(System::String^ filePath, [System::Runtime::InteropServices::Out] CompiledScript^% script);

    public:
        property System::String^ Namespace { System::String^ get(); }

        property System::Collections::Generic::Dictionary<System::String^, BundleW^>^ Bundles
        {
            System::Collections::Generic::Dictionary<System::String^, BundleW^>^ get() { return m_Bundles; }
        }

        property System::Collections::Generic::Dictionary<System::String^, FunctionW^>^ Functions
        {
            System::Collections::Generic::Dictionary<System::String^, FunctionW^>^ get() { return m_Functions; }
        }

    private:
        Nebula::Interop::SharedPtrW<nebula::Script>^ m_NativePtr;
        System::Collections::Generic::Dictionary<System::String^, BundleW^>^ m_Bundles = gcnew System::Collections::Generic::Dictionary<System::String^, BundleW^>();
        System::Collections::Generic::Dictionary<System::String^, FunctionW^>^ m_Functions = gcnew System::Collections::Generic::Dictionary<System::String^, FunctionW^>();
    };
}

