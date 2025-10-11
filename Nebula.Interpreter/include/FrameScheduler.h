#pragma once

#include "interfaces/IGCObject.h"
#include <map>

namespace nebula
{
    class Frame;
    class IGCObject;

    // Enables control of a Frame state (messaging and waiting)
    class FrameScheduler
        : public INotificationListener
    {
    public:
        FrameScheduler(Frame* parent);

        // Freeze the owned thread for X milliseconds
        void Sleep(size_t amount);

        //void WaitForNotification(IGCObject* notifier, size_t hash);
        void WaitForNotification(IGCObject* notifier, const std::string& str);

        // Returns true if the frame is sleeping, will also check time passed and clear flag
        bool IsSleeping();

        virtual bool OnNotification(IGCObject* sender, const size_t notification) override;
    private:
        Frame* m_Parent;
        size_t m_SleepAmount{ 0 };
        std::map<IGCObject*, std::unordered_set<size_t>> m_WaitingHashes;
    };
}

