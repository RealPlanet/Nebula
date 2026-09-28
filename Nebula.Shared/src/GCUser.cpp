#include <cassert>
#include <unordered_set>
#include <string>
#include <string_view>

#include "GCUser.h"
#include "NotificationListener.h"
#include "Instruction.h"

using namespace nebula;

GCUser::GCUser(ObjectType type)
	: m_ContainedType{ type }
{
}

GCUser::~GCUser()
{
	assert(m_Listeners.empty());
}

void GCUser::Subscribe(NotificationListener* listener)
{
	assert(listener);
	m_Listeners.insert(listener);
	listener->m_ConnectedNotifiers.insert(this);
}

void GCUser::Unsubscribe(NotificationListener* listener)
{
	assert(listener);
	listener->m_ConnectedNotifiers.erase(this);
	m_Listeners.erase(listener);
}

void GCUser::Unsubscribe(std::unordered_set<NotificationListener*>::iterator& it)
{
	NotificationListener* listener = *it;
	listener->m_ConnectedNotifiers.erase(this);
	it = m_Listeners.erase(it);
}

void GCUser::Notify(const std::string& notification)
{
	std::hash<std::string> hasher;
	size_t notifHash = hasher(notification);
	for (auto it = m_Listeners.begin(); it != m_Listeners.end(); it++)
	{
		bool removeListener = (*it)->OnNotification(this, notifHash);
		if (removeListener)
		{
			Unsubscribe(it);
		}

		if (it == m_Listeners.end())
		{
			break;
		}
	}
}

InstructionErrorCode nebula::GCUser::CallVirtual(const std::string_view&, nebula::Interpreter*, Frame*)
{
	return InstructionErrorCode::Fatal;
}