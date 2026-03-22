#include "FrameMemory.h"

using namespace nebula;

FrameMemory::FrameMemory(size_t paramCount, size_t localCount)
    : m_ParamCount{ paramCount }, m_LocalCount{ localCount }
{
    m_Variables = new Variable[localCount + paramCount];
}

FrameMemory::~FrameMemory()
{
    if (m_Variables)
    {
        delete[] m_Variables;
        m_Variables = nullptr;
    }
}

Variable& FrameMemory::LocalAt(size_t i)
{
    if (i > m_ParamCount + i)
    {
        throw std::exception("Invalid local index");
    }

    return m_Variables[m_ParamCount + i];
}

Variable& FrameMemory::ParamAt(size_t i)
{
    if (i > m_ParamCount)
    {
        throw std::exception("Invalid param index");
    }

    return m_Variables[i];
}

const Variable& FrameMemory::LocalAt(size_t i) const
{
    if (i > m_ParamCount + i)
    {
        throw std::exception("Invalid local index");
    }

    return m_Variables[m_ParamCount + i];
}

const Variable& FrameMemory::ParamAt(size_t i) const
{
    if (i > m_ParamCount)
    {
        throw std::exception("Invalid param index");
    }

    return m_Variables[i];
}

bool Variable::SetValue(DataStackVariant& val)
{
    if (Type() != val.index())
        return false;

    _value = val;
    return true;
}
