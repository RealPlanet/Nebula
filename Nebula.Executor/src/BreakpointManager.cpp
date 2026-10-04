#include "BreakpointManager.h"

using namespace nebula::debugger;

BreakpointInformation::BreakpointInformation(const std::string& namespace_, const std::string& functionName, const size_t& opcodeIndex)
	: m_namespace(namespace_), m_functionName(functionName), m_opcodeIndex(opcodeIndex)
{
}

bool BreakpointInformation::operator==(const BreakpointInformation& other) const
{
	return m_namespace == other.m_namespace && 
		m_functionName == other.m_functionName &&
		m_opcodeIndex == other.m_opcodeIndex;
}

void BreakpointManager::AddFunctionBreakpoint(const BreakpointInformation& breakpointInfo)
{
	bpm_lock lock(m_mutex);
	m_functionBreakpoints.insert(breakpointInfo);
}

void BreakpointManager::AddBreakpoint(const BreakpointInformation& breakpointInfo)
{
	bpm_lock lock(m_mutex);

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
	bpm_lock lock(m_mutex);
	m_functionBreakpoints.clear();
}

void BreakpointManager::ClearBreakpoints(const std::string& _namespace)
{
	bpm_lock lock(m_mutex);
	m_breakpoints[_namespace].clear();
}

void BreakpointManager::ClearBreakpoints()
{
	bpm_lock lock(m_mutex);
	m_breakpoints.clear();
}

const std::unordered_set<BreakpointInformation>& BreakpointManager::GetFunctionBreakpoints() const
{
	return m_functionBreakpoints;
}

const std::unordered_map<std::string, std::unordered_set<BreakpointInformation>>& BreakpointManager::GetBreakpoints() const
{
	return m_breakpoints;
}