#include "ThreadMap.h"
#include "Frame.h"

using namespace nebula;

void ThreadMap::RemoveCallstack(size_t index)
{
    DeleteCallstackFrames(m_Callstacks[index]);
    m_Callstacks[index] = m_Callstacks.back();
    m_Callstacks.pop_back();    
}

void ThreadMap::Clear()
{
    for (auto& stack : m_Callstacks)
        DeleteCallstackFrames(stack);

    m_Callstacks.clear();
}

void ThreadMap::DeleteCallstackFrames(CallStack& c)
{
    for (Frame* frame : c)
        delete frame;

    c.clear();
}
