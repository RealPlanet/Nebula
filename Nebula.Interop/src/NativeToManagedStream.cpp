#include "NativeToManagedStream.h"

using namespace Nebula::Interop::Utility;

NativeTextWriter::NativeTextWriter(System::IO::TextWriter^ writer)
    : _managedWriter { writer }
{
    
}

void NativeTextWriter::WriteLine(const std::string& c)
{
    _managedWriter->WriteLine(gcnew System::String(c.data()));
}

void NativeTextWriter::Write(const std::string& c)
{
    _managedWriter->Write(gcnew System::String(c.data()));
}

NativeToManagedStream::NativeToManagedStream(System::IO::TextWriter^ writer)
    : _nativeWriter { new NativeTextWriter(writer)}
{
}

NativeToManagedStream::~NativeToManagedStream()
{
    NativeToManagedStream::!NativeToManagedStream();
}

NativeToManagedStream::!NativeToManagedStream()
{
    delete _nativeWriter;
}
