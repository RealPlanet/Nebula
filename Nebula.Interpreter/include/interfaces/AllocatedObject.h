#pragma once

namespace nebula
{
    class InterpreterMemory;
    class __declspec(novtable) AllocatedObject
    {
        friend class InterpreterMemory;

    public:
        AllocatedObject() = default;
        virtual ~AllocatedObject() = default;
    };
}

