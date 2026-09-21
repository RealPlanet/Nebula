#pragma once

#include "Value.h"

namespace nebula
{
    class FrameMemory
    {
    public:
        FrameMemory(size_t paramCount, size_t localCount);
        FrameMemory(FrameMemory&& f) = delete;
        FrameMemory(const FrameMemory&) = delete; // No copy allowed
        ~FrameMemory();

        Value& LocalAt(size_t i);
        Value& ParamAt(size_t i);
        const Value& LocalAt(size_t i) const;
        const Value& ParamAt(size_t i) const;

        size_t ParamCount() const { return m_ParamCount; }
        size_t LocalCount() const { return m_LocalCount; }

    private:
        Value* m_Variables{ nullptr };
        size_t m_ParamCount{ 0 };
        size_t m_LocalCount{ 0 };
    };
}

