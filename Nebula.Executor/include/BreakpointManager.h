#pragma once

#ifndef _H_BREAKPOINT_MANAGER_
#define _H_BREAKPOINT_MANAGER_

#include "DebugTypes.h"

#include <string>
#include <unordered_map>
#include <unordered_set>
#include <mutex>

namespace nebula::debugger
{
	class BreakpointManager
	{
		using mutex = std::recursive_mutex;
		using bpm_lock = std::scoped_lock<mutex>;

		using bp_set = std::unordered_set<BreakpointInformation>;
		using bp_map = std::unordered_map<std::string, std::unordered_set<BreakpointInformation>>;

	public:
		void AddFunctionBreakpoint(const BreakpointInformation& breakpointInfo);
		void AddBreakpoint(const BreakpointInformation& breakpointInfo);
		void ClearFunctionBreakpoints();
		void ClearBreakpoints(const std::string& _namespace);
		void ClearBreakpoints();

		mutex& GetMutex() const { return m_mutex; }
		const bp_set& GetFunctionBreakpoints() const;
		const bp_map& GetBreakpoints() const;

	private:
		mutable mutex m_mutex;
		bp_set m_functionBreakpoints;
		bp_map m_breakpoints;
	};
} // namespace nebula::debugger



#endif // !_H_BREAKPOINT_MANAGER_
