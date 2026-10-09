#pragma once

#include "CallStack.h"

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
        CallStack& At(size_t index) { return m_Callstacks.at(index); }
        const CallStack& At(size_t index) const { return m_Callstacks.at(index); }

        CallStack* CreateNewThread() { return &m_Callstacks.emplace_back(); }
        bool HasCallStacks() { return !m_Callstacks.empty(); }

        constexpr auto begin() noexcept { return m_Callstacks.begin(); }
        constexpr auto begin() const noexcept { return m_Callstacks.begin(); }
        constexpr auto cbegin() const noexcept { return m_Callstacks.cbegin(); }
        constexpr auto end() noexcept { return m_Callstacks.end(); }
        constexpr auto end() const noexcept { return m_Callstacks.end(); }
        constexpr auto cend() const noexcept{ return m_Callstacks.cend(); }

    private:
        void DeleteCallstackFrames(CallStack&);

        CallstackVector m_Callstacks;
    };
}

