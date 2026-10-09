#pragma once

#ifndef _H_NEBULA_INSTRUCTION_
#define _H_NEBULA_INSTRUCTION_

#include <format>
#include <string>
#include <vector>

#include "LanguageTypes.h"

namespace nebula
{
	using RawArguments = std::vector<std::string>;
	using InstructionArguments = std::vector<DataStackVariant>;

	enum class InstructionErrorCode
	{
		Unknown,
		None,
		Fatal,
		FunctionNotFound,
		NativeFunctionNotFound,
		BundleNotFound,
		DivideByZero,
		EndonKilledUnthreadedFunctionWithReturnValue,
		NotAPrimitive,
		GlobalVariableNotFound,

		// GC Objects
		UndefinedObject,
		NotABundle,
		NotAnArray,
	};

	inline std::string InstructionErrorCodeToString(InstructionErrorCode c)
	{
		switch (c)
		{
		case InstructionErrorCode::Unknown:
			return "Unknown";
		case InstructionErrorCode::None:
			return "None";
		case InstructionErrorCode::Fatal:
			return "Fatal";
		case InstructionErrorCode::FunctionNotFound:
			return "FunctionNotFound";
		case InstructionErrorCode::NativeFunctionNotFound:
			return "NativeFunctionNotFound";
		case InstructionErrorCode::BundleNotFound:
			return "BundleNotFound";
		case InstructionErrorCode::DivideByZero:
			return "DivideByZero";
		case InstructionErrorCode::EndonKilledUnthreadedFunctionWithReturnValue:
			return "EndonKilledUnthreadedFunctionWithReturnValue";
		case InstructionErrorCode::NotAPrimitive:
			return "NotAPrimitive";
		case InstructionErrorCode::GlobalVariableNotFound:
			return "GlobalVariableNotFound";
		case InstructionErrorCode::UndefinedObject:
			return "UndefinedObject";
		case InstructionErrorCode::NotABundle:
			return "NotABundle";
		case InstructionErrorCode::NotAnArray:
			return "NotAnArray";
		}

		return std::format("Unknown error code: {}", (int)c);
	}
} // namespace nebula

#endif // !_H_NEBULA_INSTRUCTION_
