#pragma once

#include <string>
#include <hash_set>

namespace nebula
{
    class AwaitableObject
    {
    public:
        void WaitFor(size_t hash);
        void WaitFor(const std::string& str);
        bool IsWaiting() const;

    protected:
        std::hash_set<size_t> m_Notifications;
    };
}