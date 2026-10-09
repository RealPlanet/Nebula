#pragma once

#ifndef _H_NEBULA_GC_USER_
#define _H_NEBULA_GC_USER_

#include "NotificationListener.h"
#include "Instruction.h"

#include <string>
#include <string_view>
#include <unordered_set>

namespace nebula
{
    class Interpreter;
    class Frame;

    enum class ObjectType
    {
        Undefined,
        Bundle,
        Array,
    };

    class __declspec(novtable) GCUser {
        friend class InterpreterMemory;
    public:
        GCUser(ObjectType type);

        inline ObjectType GetType() { return m_ContainedType; }

        void Subscribe(NotificationListener*);
        void Unsubscribe(NotificationListener*);

        void Notify(const std::string& notification);

        virtual InstructionErrorCode CallVirtual(const std::string_view& name, std::vector<DataStackVariant>& arguments, nebula::Interpreter* vm, Frame* context);

    protected:
        void Unsubscribe(std::unordered_set<NotificationListener*>::iterator&);
        std::unordered_set<NotificationListener*> m_Listeners;

        /// <summary> Used by the GC to mark reachable objects </summary>
        bool m_bIsMarked{ false };
        ObjectType m_ContainedType;

    public:
        virtual ~GCUser();
    };
}

#endif // !_H_NEBULA_GC_USER_

