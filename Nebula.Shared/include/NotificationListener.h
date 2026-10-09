#pragma once

#ifndef _H_NEBULA_NOTIFICATION_LISTENER_
#define _H_NEBULA_NOTIFICATION_LISTENER_

#include <unordered_set>

namespace nebula
{
    class GCUser;

    class NotificationListener
    {
        friend class GCUser;

    public:
        //virtual void OnNotification(const std::string& notification) = 0;
        virtual bool OnNotification(GCUser* sender, const size_t notification) = 0;
        virtual ~NotificationListener();

    protected:
        void UnsubscribeFromAll();

    private:
        std::unordered_set<GCUser*> m_ConnectedNotifiers;
    };
}

#endif // !_H_NEBULA_NOTIFICATION_LISTENER_
