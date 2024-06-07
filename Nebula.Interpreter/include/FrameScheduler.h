#pragma once

namespace nebula
{
    class Frame;

    /*
    *   TODO :: While currently only wait instructions are supported, ideally, bundle variables (since they're passed by reference) should be able to wait and awake on specific notifications
    *       to stop and continue thread execution.
    */

    // Enables control of a Frame state (messaging and waiting)
    class FrameScheduler
    {
    public:
        FrameScheduler(Frame* parent);

        // Freeze the owned thread for X milliseconds
        void Sleep(size_t amount);

        // Returns true if the frame is sleeping, will also check time passed and clear flag
        bool IsSleeping();

    private:
        Frame* m_Parent;
        size_t m_SleepAmount{ 0 };
    };
}

