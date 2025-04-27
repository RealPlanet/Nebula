#include <cassert>

#include "interfaces/INotificationListener.h"
#include "interfaces/IGCObject.h"

using namespace nebula;

INotificationListener::~INotificationListener()
{
    UnsubscribeFromAll();
}

void INotificationListener::UnsubscribeFromAll()
{
    for (IGCObject* notifier : m_ConnectedNotifiers)
    {
        notifier->Unsubscribe(this);
    }

    m_ConnectedNotifiers.clear();
}
