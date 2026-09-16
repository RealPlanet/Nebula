#pragma once

#ifndef _H_BREAKPOINT_MANAGER_
#define _H_BREAKPOINT_MANAGER_

#include "DebugTypes.h"

#include <string>
#include <shared_mutex>
#include <unordered_map>
#include <unordered_set>

namespace nebula::debugger
{
	class BreakpointManager
	{
	public:
		void AddFunctionBreakpoint(const BreakpointInformation& breakpointInfo);
		void AddBreakpoint(const BreakpointInformation& breakpointInfo);
		void ClearFunctionBreakpoints();
		void ClearBreakpoints(const std::string& _namespace);
		void ClearBreakpoints();

		std::scoped_lock<std::shared_mutex> Lock() const { return std::scoped_lock<std::shared_mutex>{ m_mutex }; }

		const std::unordered_set<BreakpointInformation>& GetFunctionBreakpoints() const { return m_functionBreakpoints; }
		const std::unordered_map<std::string, std::unordered_set<BreakpointInformation>>& GetBreakpoints() const { return m_breakpoints; }
	private:
		mutable std::shared_mutex m_mutex;
		std::unordered_set<BreakpointInformation> m_functionBreakpoints;
		std::unordered_map<std::string, std::unordered_set<BreakpointInformation>> m_breakpoints;
	};
} // namespace nebula::debugger



#endif // !_H_BREAKPOINT_MANAGER_
