#pragma once

#include "Callstack.h"

#include <map>
#include <vector>

namespace nebula
{

    class ThreadMap
    {
    public:
        void RemoveCallstack(size_t index);
        void Clear();

        size_t Count() const { return m_Callstacks.size(); }
        CallStack& At(size_t index) { return m_Callstacks[index]; }
        CallStack* CreateNewThread() { return &m_Callstacks.emplace_back(); }
        bool HasCallStacks() { return !m_Callstacks.empty(); }

    private:
        void DeleteCallstackFrames(CallStack&);

        CallstackVector m_Callstacks;
    };
}

