#pragma once

#include <vector>

namespace nebula
{
    class Frame;

    using CallStack = std::vector<Frame*>;
    using CallstackVector = std::vector<CallStack>;
}

