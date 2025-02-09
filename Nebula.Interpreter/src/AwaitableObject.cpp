#include <cassert>

#include "interfaces/AwaitableObject.h"

using namespace nebula;

AwaitableObject::~AwaitableObject()
{
    assert(m_Listeners.empty());
}

void AwaitableObject::Subscribe(INotificationListener* listener)
{
    assert(listener);
    m_Listeners.insert(listener);
    listener->m_ConnectedNotifiers.insert(this);
}

void AwaitableObject::Unsubscribe(INotificationListener* listener)
{
    assert(listener);
    listener->m_ConnectedNotifiers.erase(this);
    m_Listeners.erase(listener);
}

void AwaitableObject::Unsubscribe(std::unordered_set<INotificationListener*>::iterator& it)
{
    INotificationListener* listener = *it;
    listener->m_ConnectedNotifiers.erase(this);
    it = m_Listeners.erase(it);
}

void AwaitableObject::Notify(const std::string& notification)
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

//void AwaitableObject::Notify(const size_t& notification)
//{
//    for (auto it = m_Listeners.begin(); it != m_Listeners.end();)
//    {
//        bool removeListener = (*it)->OnNotification(this, notification);
//        if (removeListener)
//        {
//            Unsubscribe(it);
//        }
//
//        if (it == m_Listeners.end())
//        {
//            break;
//        }
//    }
//}

INotificationListener::~INotificationListener()
{
    UnsubscribeFromAll();
}

void INotificationListener::UnsubscribeFromAll()
{
    for (AwaitableObject* notifier : m_ConnectedNotifiers)
    {
        notifier->Unsubscribe(this);
    }

    m_ConnectedNotifiers.clear();
}
