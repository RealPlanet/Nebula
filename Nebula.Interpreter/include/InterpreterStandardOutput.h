#pragma once

#include "interfaces\IStreamWrapper.h"

namespace nebula
{
    class InterpreterStandardOutput
        : public IStreamWrapper
    {
    public:
        virtual ~InterpreterStandardOutput() = default;

        virtual void WriteLine(const std::string& c) override;
        virtual void Write(const std::string& c) override;
    };
}