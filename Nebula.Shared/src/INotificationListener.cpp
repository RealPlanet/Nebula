#include "interfaces/INotificationListener.h"
#include "interfaces/IGCObject.h"

using namespace nebula;

INotificationListener::~INotificationListener()
{
    UnsubscribeFromAll();
}

void INotificationListener::UnsubscribeFromAll()
{
    std::unordered_set<IGCObject*> notifiers = { m_ConnectedNotifiers };
    for (IGCObject* obj : notifiers)
    {
        obj->Unsubscribe(this);
    }

    m_ConnectedNotifiers.clear();
}
