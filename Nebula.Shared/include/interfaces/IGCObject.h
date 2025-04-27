#pragma once

#include "INotificationListener.h"

#include <string>
#include <unordered_set>

namespace nebula
{
    class __declspec(novtable) IGCObject {
        friend class InterpreterMemory;
    public:
        void Subscribe(INotificationListener*);
        void Unsubscribe(INotificationListener*);

        void Notify(const std::string& notification);

    protected:
        void Unsubscribe(std::unordered_set<INotificationListener*>::iterator&);
        std::unordered_set<INotificationListener*> m_Listeners;


        /// <summary> Used by the GC to mark reachable objects </summary>
        bool m_bIsMarked{ false };

    public:
        IGCObject() = default;
        virtual ~IGCObject();
    };
}

