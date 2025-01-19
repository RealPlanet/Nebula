#include "CompiledScript.h"
#include "Script.h"

#include <string>
#include <msclr\marshal_cppstd.h>

using namespace Nebula::Interop;

bool CompiledScript::LoadScriptFromFile(System::String^ filePath, [System::Runtime::InteropServices::Out] CompiledScript^% script)
{
    System::String^ scriptContents = System::IO::File::ReadAllText(filePath);
    std::string stdString = msclr::interop::marshal_as<std::string>(scriptContents);
    nebula::ScriptLoadResult result = nebula::Script::FromMemory(stdString);

    if (result.Script != nullptr)
    {
        script = gcnew CompiledScript();
        script->m_NativePtr = gcnew Nebula::Interop::SharedPtrW<nebula::Script>(result.Script);


        for (auto& kvp : script->m_NativePtr->Bundles())
        {
            System::String^ bundleName = gcnew System::String(kvp.first.c_str());
            BundleW^ bundleWrapper = gcnew BundleW(&kvp.second);
            script->m_Bundles->Add(bundleName, bundleWrapper);
        }

        for (auto& kvp : script->m_NativePtr->Functions())
        {
            System::String^ funcName = gcnew System::String(kvp.first.c_str());
            FunctionW^ funcWrapper = gcnew FunctionW(&kvp.second);
            script->m_Functions->Add(funcName, funcWrapper);
        }

        return true;
    }

    script = nullptr;
    return false;
}

System::String^ CompiledScript::Namespace::get()
{
    return gcnew System::String(m_NativePtr->Namespace().c_str());
}
