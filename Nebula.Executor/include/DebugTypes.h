#pragma once

#ifndef _H_DEBUG_TYPES_
#define _H_DEBUG_TYPES_

#include "Frame.h"
#include "DebugFile.h"
#include "DebugVariable.h"

#include <cstdint>
#include <string>

namespace nebula::debugger
{
	using ThreadId = uint64_t;
	using FrameId = uint64_t;
	using VariableId = uint64_t;
	constexpr ThreadId NO_THREAD_ID = (uint64_t)-1;

	enum class ContinueResult
	{
		Unkown,
		Error,
		Hitbreakpoint,
		Done
	};

	enum class DebugStartRequestType
	{
		Unkown,
		Attach,
		Launch
	};

	enum class ScopeType
	{
		Generic,
		Arguments,
		Locals,
	};

	struct DebugBreakpoint
	{
		ThreadId threadId;
		bool isFunctionBreakpoint;
	};

	struct DebugEventInfo
	{
		enum class Event
		{
			Unkown,
			Step,
			StepIn,
			Continue,
		};

		Event type;
		ThreadId threadId;
	};

	struct DebugThread
	{
		ThreadId id;
		std::string name;
		size_t frameCount;
	};

	struct DebugFrame
	{
		FrameId id;
		std::string functionNamespace;
		std::string functionName;
		size_t line;
		size_t column;

		nebula::Frame* originalFrame;
	};

	struct DebugScope
	{
		const DebugFrame* frame;

		std::string name;
		VariableId reference;
		ScopeType type;
	};

	struct DebugVariable
	{
		enum class Scope
		{
			Local,
			Arguments,
			Global,
			Member,
			ArrayElement,
		};

		// Variable reference if this variable holds child variables
		VariableId reference;
		// The scope of this variable, if it is a local, parameter or something else..
		Scope scope;
		// The read debug information
		const nebula::debugger::symbols::DebugVariable* debugInformation;
		// If this value can be changed by a debugger, for now only primitives are allowed
		bool canChangeValueByDebugger;

		nebula::Variable* originalVariable;

		bool OverrideValue(const std::string& valueToSet, std::string& reason);
		std::string GetDisplayValue() const;
		std::string GetDisplayType() const;
	};

	struct DebugSource
	{
		std::string name;
		size_t reference;
		std::string namespace_;
		std::string path;
		std::string origin;
	};

	class BreakpointInformation
	{
	public:
		inline static size_t NO_OPCODE = (size_t)-1;

	public:
		BreakpointInformation(const std::string& m_namespace, const std::string& m_functionName,
			const size_t& m_opcodeIndex)
			: m_namespace(m_namespace), m_functionName(m_functionName), m_opcodeIndex(m_opcodeIndex)
		{
		}

		bool operator==(const BreakpointInformation& other) const
		{
			return m_namespace == other.m_namespace && m_functionName == other.m_functionName &&
				m_opcodeIndex == other.m_opcodeIndex;
		}

		const std::string& GetNamespace() const { return m_namespace; }
		const std::string& GetFunctionName() const { return m_functionName; }
		size_t GetOpcodeIndex() const { return m_opcodeIndex; }
	private:
		std::string m_namespace;
		std::string m_functionName;
		size_t m_opcodeIndex;
	};

	struct DebugOutput
	{
		std::string output;
		std::string functionNamespace;
		std::string functionName;
		size_t line;

		const nebula::debugger::symbols::DebugFile* fileSymbols;
		const nebula::debugger::symbols::DebugFunction* functionSymbols;
	};
}

template<>
struct ::std::hash<nebula::debugger::BreakpointInformation>
{
	size_t operator()(const nebula::debugger::BreakpointInformation& value) const
	{
		return std::hash<size_t>{}(value.GetOpcodeIndex()) ^
			std::hash<std::string>{}(value.GetNamespace()) ^
			std::hash<std::string>{}(value.GetFunctionName());
	}
};


#endif // !_H_DEBUG_TYPES_


