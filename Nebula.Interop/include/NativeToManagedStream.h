#pragma once

#include "interfaces\IStreamWrapper.h"

#include <iostream>
#include <msclr/gcroot.h>

namespace Nebula::Interop::Utility
{
    class NativeTextWriter
        : public nebula::IStreamWrapper
    {
    public:
        NativeTextWriter(System::IO::TextWriter^ writer);

        virtual void WriteLine(const std::string& c) override;
        virtual void Write(const std::string& c) override;

    private:
        msclr::gcroot<System::IO::TextWriter^> _managedWriter;
    };

    public ref class NativeToManagedStream
    {
    public:
        NativeToManagedStream(System::IO::TextWriter^ writer);
        ~NativeToManagedStream();
        !NativeToManagedStream();

        NativeTextWriter* GetNativeStream() { return _nativeWriter; }
    private:
        NativeTextWriter* _nativeWriter;
    };
}