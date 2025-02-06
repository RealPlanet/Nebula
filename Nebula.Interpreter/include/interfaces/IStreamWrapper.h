#pragma once

#include <string>

namespace nebula
{
    class IStreamWrapper
    {
    public:
        IStreamWrapper() = default;
        virtual ~IStreamWrapper() = default;

        virtual void WriteLine(const std::string& line) = 0;
        virtual void Write(const std::string& line) = 0;
    };
}



