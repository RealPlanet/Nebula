#pragma once

#include "AwaitableObject.h"
#include <map>

namespace nebula
{
    class Frame;
    class AwaitableObject;

    /*
    *   TODO :: While currently only wait instructions are supported, ideally, bundle variables (since they're passed by reference) should be able to wait and awake on specific notifications
    *       to stop and continue thread execution.
    */

    // Enables control of a Frame state (messaging and waiting)
    class FrameScheduler
        : public INotificationListener
    {
    public:
        FrameScheduler(Frame* parent);

        // Freeze the owned thread for X milliseconds
        void Sleep(size_t amount);

        //void WaitForNotification(AwaitableObject* notifier, size_t hash);
        void WaitForNotification(AwaitableObject* notifier, const std::string& str);

        // Returns true if the frame is sleeping, will also check time passed and clear flag
        bool IsSleeping();

        virtual bool OnNotification(AwaitableObject* sender, const size_t notification) override;
    private:
        Frame* m_Parent;
        size_t m_SleepAmount{ 0 };
        std::map<AwaitableObject*, std::unordered_set<size_t>> m_WaitingHashes;
    };
}

