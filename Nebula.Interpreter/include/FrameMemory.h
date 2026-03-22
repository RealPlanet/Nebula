#pragma once

#include "Variable.h"

namespace nebula
{
    class FrameMemory
    {
    public:
        FrameMemory(size_t paramCount, size_t localCount);
        FrameMemory(FrameMemory&& f) = delete;
        FrameMemory(const FrameMemory&) = delete; // No copy allowed
        ~FrameMemory();

        Variable& LocalAt(size_t i);
        Variable& ParamAt(size_t i);
        const Variable& LocalAt(size_t i) const;
        const Variable& ParamAt(size_t i) const;

        size_t ParamCount() const { return m_ParamCount; }
        size_t LocalCount() const { return m_LocalCount; }

    private:
        Variable* m_Variables{ nullptr };
        size_t m_ParamCount{ 0 };
        size_t m_LocalCount{ 0 };
    };
}

