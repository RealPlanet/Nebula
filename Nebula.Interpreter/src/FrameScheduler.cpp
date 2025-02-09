#include "Utility.h"
#include "FrameScheduler.h"
#include "interfaces/AwaitableObject.h"

using namespace nebula;

static void AddOrUpdateWaitingHashSet(std::map<AwaitableObject*, std::unordered_set<size_t>>& map, AwaitableObject* notifier, size_t hash)
{
    auto it = map.find(notifier);
    if (it == map.end())
    {
        std::unordered_set<size_t> set{};
        set.insert(hash);
        map.insert(std::make_pair(notifier, set));
    }
    else
    {
        it->second.insert(hash);
    }
}

FrameScheduler::FrameScheduler(Frame* parent)
    : m_Parent{ parent }
{
    assert(parent);
}

void FrameScheduler::Sleep(size_t amount)
{
    m_SleepAmount = GetCurrentMillis() + amount;
}

//void FrameScheduler::WaitForNotification(AwaitableObject* notifier, size_t hash)
//{
//    AddOrUpdateWaitingHashSet(m_WaitingHashes, notifier, hash);
//    notifier->Subscribe(this);
//}

void FrameScheduler::WaitForNotification(AwaitableObject* notifier, const std::string& str)
{
    std::hash<std::string> hasher;
    size_t hash = hasher(str);
    AddOrUpdateWaitingHashSet(m_WaitingHashes, notifier, hash);
    notifier->Subscribe(this);
}

bool FrameScheduler::IsSleeping()
{
    if (m_SleepAmount > 0)
    {
        if (GetCurrentMillis() < m_SleepAmount)
            return true;

        m_SleepAmount = 0;
    }

    if (m_WaitingHashes.size() > 0)
        return true;

    return false;
}

bool FrameScheduler::OnNotification(AwaitableObject* sender, const size_t notification)
{
    auto it = m_WaitingHashes.find(sender);
    if (it == m_WaitingHashes.end())
        return true;

    it->second.erase(notification);
    if (it->second.empty())
    {
        m_WaitingHashes.erase(it);
        return true;
    }

    return false;
}
