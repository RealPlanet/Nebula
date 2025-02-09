#pragma once

#include <string>
#include <unordered_set>

namespace nebula
{
    class AwaitableObject;

    class __declspec(novtable) INotificationListener
    {
        friend class AwaitableObject;

    public:
        //virtual void OnNotification(const std::string& notification) = 0;
        virtual bool OnNotification(AwaitableObject* sender, const size_t notification) = 0;
        virtual ~INotificationListener();

    protected:
        void UnsubscribeFromAll();

    private:
        std::unordered_set<AwaitableObject*> m_ConnectedNotifiers;
    };

    class AwaitableObject
    {
    public:
        virtual ~AwaitableObject();

        void Subscribe(INotificationListener*);
        void Unsubscribe(INotificationListener*);

        void Notify(const std::string& notification);
        //void Notify(const size_t& notification);

    protected:   
        void Unsubscribe(std::unordered_set<INotificationListener*>::iterator&);


        std::unordered_set<INotificationListener*> m_Listeners;
    };
}