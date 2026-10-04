#pragma once

#ifndef _H_NEBULA_DEBUGGER_ENTITIES_
#define _H_NEBULA_DEBUGGER_ENTITIES_

#include <cstdint>
#include <string>
#include <vector>

#include "DebugSymbols.h"

// Executor classes to wrap
#include "Value.h"
#include "Callstack.h"

namespace nebula::debugger
{
	using ThreadId = uint64_t;
	using FrameId = uint64_t;
	using VariableId = uint64_t;

	constexpr ThreadId INVALID_THREAD_ID = (ThreadId)-1;
	constexpr FrameId INVALID_FRAME_ID = (ThreadId)-1;
	constexpr VariableId INVALID_VARIABLE_ID = (ThreadId)-1;

	enum DebugStartRequestType
	{
		Unkown,
		Attach,
		Launch
	};

	struct Breakpoint
	{
		ThreadId threadId;
		bool isFunctionBreakpoint;
	};

	struct Source
	{
		std::string name{};
		size_t reference{ 0 };
		std::string namespace_{};
		std::string path{};
		std::string origin{};
		const symbols::DebugSymbols* debugSymbols{ nullptr };
	};

	struct Thread
	{
		ThreadId id{ INVALID_THREAD_ID };
		std::string name{};

		const nebula::CallStack* internalThread{ nullptr };
	};

	struct Frame
	{
		FrameId id{ INVALID_FRAME_ID };
		size_t currentLine{ 0 };
		nebula::Frame* internalFrame{ nullptr };

		const Source* source{ nullptr };
		const symbols::FunctionInformation* functionSymbols{ nullptr };
	};

	struct Value
	{
		VariableId id{ INVALID_VARIABLE_ID };
		const symbols::DebugSymbols* debugSymbols;
		const symbols::TypeInformation* typeInformation;
		// Explicit copy so we can support on the fly array values
		const symbols::ValueInformation valueInformation;

		nebula::Value* internalValue{ nullptr };

		bool CanDebuggerChangeValue() const;
		bool OverrideValue(const std::string& newValue, std::string& failReason);

		std::string GetDisplayValue() const;

	private:
		bool OverrideBoolValue(const std::string& newValue, std::string& failReason);
		bool OverrideInt32Value(const std::string& newValue, std::string& failReason);
		bool OverrideFloatValue(const std::string& newValue, std::string& failReason);
		bool OverrideStringValue(const std::string& newValue, std::string& failReason);
	};

	struct GenericScope
	{
		enum class eType
		{
			NotSpecific = 0,
			Arguments,
			Locals,
		};

		VariableId id{ 0 };
		std::string name;
		eType type;

		const nebula::Frame* internalFrame;
	};

	struct GlobalScope
	{
		VariableId id{ 0 };
		std::string name;

		//std::vector <
	};

	struct InterpreterOutput
	{
		std::string output{};
		std::string functionNamespace{};
		std::string functionName{};
		size_t line{};

		const symbols::DebugSymbols* fileSymbols{ nullptr };
		const symbols::FunctionInformation* functionSymbols{ nullptr };
	};
}

#endif // !_H_NEBULA_DEBUGGER_ENTITIES_
