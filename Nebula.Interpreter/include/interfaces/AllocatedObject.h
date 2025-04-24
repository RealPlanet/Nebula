#pragma once

namespace nebula
{
    class InterpreterMemory;
    class __declspec(novtable) AllocatedObject
    {
        friend class InterpreterMemory;

    protected:
        bool m_bIsMarked{ false };

    public:
        AllocatedObject() = default;
        virtual ~AllocatedObject() = default;
    };
}

