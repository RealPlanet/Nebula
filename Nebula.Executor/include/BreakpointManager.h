#pragma once

#ifndef _H_NEBULA_BREAKPOINT_MANAGER_
#define _H_NEBULA_BREAKPOINT_MANAGER_

#include <mutex>
#include <string>
#include <unordered_map>
#include <unordered_set>

namespace nebula::debugger
{
	class BreakpointInformation
	{
	public:
		inline static size_t NO_OPCODE = (size_t)-1;

	public:
		BreakpointInformation(const std::string& namespace_, const std::string& functionName, const size_t& opcodeIndex);

		bool operator==(const BreakpointInformation& other) const;
		bool operator!=(const BreakpointInformation& other) const { return !(*this == other); }

		const std::string& GetNamespace() const { return m_namespace; }
		const std::string& GetFunctionName() const { return m_functionName; }
		size_t GetOpcodeIndex() const { return m_opcodeIndex; }

	private:
		std::string m_namespace;
		std::string m_functionName;
		size_t m_opcodeIndex;
	};
}

template<>
struct ::std::hash<nebula::debugger::BreakpointInformation>
{
	inline size_t operator()(const nebula::debugger::BreakpointInformation& value) const
	{
		return std::hash<size_t>{}(value.GetOpcodeIndex()) ^
			std::hash<std::string>{}(value.GetNamespace()) ^
			std::hash<std::string>{}(value.GetFunctionName());
	}
};

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

		mutex& GetMutex() const
		{
			return m_mutex;
		}
		const bp_set& GetFunctionBreakpoints() const;
		const bp_map& GetBreakpoints() const;

	private:
		mutable mutex m_mutex;
		bp_set m_functionBreakpoints;
		bp_map m_breakpoints;
	};
} // namespace nebula::debugger

#endif // !_H_NEBULA_BREAKPOINT_MANAGER_
