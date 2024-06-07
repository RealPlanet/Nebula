#include "Utility.h"
#include "FrameScheduler.h"

using namespace nebula;

FrameScheduler::FrameScheduler(Frame* parent)
    : m_Parent{ parent }
{
    assert(parent);
}

void nebula::FrameScheduler::Sleep(size_t amount)
{
    m_SleepAmount = GetCurrentMillis() + amount;
}

bool FrameScheduler::IsSleeping()
{
    if (m_SleepAmount > 0)
    {
        if (GetCurrentMillis() < m_SleepAmount)
            return true;

        m_SleepAmount = 0;
    }

    return false;
}
