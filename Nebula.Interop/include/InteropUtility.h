#pragma once

#include <string>

namespace Nebula::Interop
{
    /// <summary>
    /// Converts a System::String to std::string
    /// </summary>
    inline void MarshalString(System::String^ s, std::string& os) {
        using namespace System::Runtime::InteropServices;
        const char* chars =
            (const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
        os = chars;
        Marshal::FreeHGlobal(System::IntPtr((void*)chars));
    }
}