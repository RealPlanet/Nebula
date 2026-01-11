#include <cassert>

#include "Utility.h"
#include "FrameScheduler.h"
#include "interfaces/IGCObject.h"
#include "Frame.h"

using namespace nebula;

static std::hash<std::string> gStringHasher;

static void AddOrUpdateWaitingHashSet(std::map<IGCObject*, std::unordered_set<size_t>>& map, IGCObject* notifier, size_t hash)
{
	auto it = map.find(notifier);
	if (it == map.end())
	{
		std::unordered_set<size_t> set{ hash };
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

void FrameScheduler::Kill()
{
	m_Killed = true;
	m_WaitingEndonHashes.clear();
}

void FrameScheduler::WaitForNotification(IGCObject* notifier, const std::string& notification)
{
	size_t hash = gStringHasher(notification);
	AddOrUpdateWaitingHashSet(m_WaitingHashes, notifier, hash);
	notifier->Subscribe(this);
}

void FrameScheduler::EndOnNotification(IGCObject* notifier, const std::string& notification)
{
	size_t hash = gStringHasher(notification);

	AddOrUpdateWaitingHashSet(m_WaitingEndonHashes, notifier, hash);
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

bool FrameScheduler::OnNotification(IGCObject* sender, const size_t notification)
{
	// We are either waiting to unlock sleep or kill the frame, cant have both (doesn't make sense so wait always wins)
	bool foundListener = FindAndRemoveWaitingHash(sender, notification);
	bool foundEndon = FindAndRemoveEndonHash(sender, notification);

	// If we don't find neither we need to unsubscribe as we have no use for it
	return !foundListener && !foundEndon;
}

bool FrameScheduler::FindAndRemoveWaitingHash(IGCObject* sender, const size_t notification)
{
	auto it = m_WaitingHashes.find(sender);
	if (it == m_WaitingHashes.end())
	{
		return false;
	}

	it->second.erase(notification);
	if (it->second.empty())
	{
		m_WaitingHashes.erase(it);
		return true;
	}

	return false;
}

bool FrameScheduler::FindAndRemoveEndonHash(IGCObject* sender, const size_t notification)
{
	auto it = m_WaitingEndonHashes.find(sender);
	if (it == m_WaitingEndonHashes.end())
		return false;

	it->second.erase(notification);
	m_WaitingEndonHashes.clear(); // Clear all, we are about to deallocate anyways because the frame is about to die
	m_Killed = true;
	return true;
}
