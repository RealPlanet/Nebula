#include "BreakpointManager.h"

using namespace nebula::debugger;

void BreakpointManager::AddFunctionBreakpoint(const BreakpointInformation& breakpointInfo)
{
	std::unique_lock lock(m_mutex);
	m_functionBreakpoints.insert(breakpointInfo);
}

void BreakpointManager::AddBreakpoint(const BreakpointInformation& breakpointInfo)
{
	std::unique_lock lock(m_mutex);

	auto it = m_breakpoints.find(breakpointInfo.GetNamespace());
	if (it == m_breakpoints.end())
	{
		m_breakpoints.insert(std::make_pair(breakpointInfo.GetNamespace(),
			std::unordered_set<BreakpointInformation>{ breakpointInfo }));

		return;
	}

	it->second.insert(breakpointInfo);
}

void BreakpointManager::ClearFunctionBreakpoints()
{
	std::unique_lock lock(m_mutex);
	m_functionBreakpoints.clear();
}

void BreakpointManager::ClearBreakpoints(const std::string& _namespace)
{
	std::unique_lock lock(m_mutex);
	m_breakpoints[_namespace].clear();
}

void BreakpointManager::ClearBreakpoints()
{
	std::unique_lock lock(m_mutex);
	m_breakpoints.clear();
}
