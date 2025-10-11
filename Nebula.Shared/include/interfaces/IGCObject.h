#pragma once

#include "INotificationListener.h"
#include "Instruction.h"

#include <string>
#include <unordered_set>

namespace nebula
{
    class Interpreter;
    class Frame;

    class __declspec(novtable) IGCObject {
        friend class InterpreterMemory;
    public:
        void Subscribe(INotificationListener*);
        void Unsubscribe(INotificationListener*);

        void Notify(const std::string& notification);

        virtual InstructionErrorCode CallVirtual(const std::string_view& name, nebula::Interpreter* vm, Frame* context);
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

