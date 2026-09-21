#include "FrameMemory.h"

using namespace nebula;

FrameMemory::FrameMemory(size_t paramCount, size_t localCount)
	: m_ParamCount{ paramCount }, m_LocalCount{ localCount }
{
	m_Variables = new Value[localCount + paramCount];
}

FrameMemory::~FrameMemory()
{
	if (m_Variables)
	{
		delete[] m_Variables;
		m_Variables = nullptr;
	}
}

Value& FrameMemory::LocalAt(size_t i)
{
	if (i > m_ParamCount + i)
	{
		throw std::exception("Invalid local index");
	}

	return m_Variables[m_ParamCount + i];
}

Value& FrameMemory::ParamAt(size_t i)
{
	if (i > m_ParamCount)
	{
		throw std::exception("Invalid param index");
	}

	return m_Variables[i];
}

const Value& FrameMemory::LocalAt(size_t i) const
{
	if (i > m_ParamCount + i)
	{
		throw std::exception("Invalid local index");
	}

	return m_Variables[m_ParamCount + i];
}

const Value& FrameMemory::ParamAt(size_t i) const
{
	if (i > m_ParamCount)
	{
		throw std::exception("Invalid param index");
	}

	return m_Variables[i];
}