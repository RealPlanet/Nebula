#pragma once

#include <string>
#include <unordered_set>

namespace nebula
{
    class IGCObject;

    class __declspec(novtable) INotificationListener
    {
        friend class IGCObject;

    public:
        //virtual void OnNotification(const std::string& notification) = 0;
        virtual bool OnNotification(IGCObject* sender, const size_t notification) = 0;
        virtual ~INotificationListener();

    protected:
        void UnsubscribeFromAll();

    private:
        std::unordered_set<IGCObject*> m_ConnectedNotifiers;
    };
}