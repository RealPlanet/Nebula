#include "NotificationListener.h"
#include "GCUser.h"

#include <unordered_set>

using namespace nebula;

NotificationListener::~NotificationListener()
{
    UnsubscribeFromAll();
}

void NotificationListener::UnsubscribeFromAll()
{
    std::unordered_set<GCUser*> notifiers = { m_ConnectedNotifiers };
    for (GCUser* obj : notifiers)
    {
        obj->Unsubscribe(this);
    }

    m_ConnectedNotifiers.clear();
}
