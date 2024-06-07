#pragma once

#include <memory>
#include <variant>
#include <string>

namespace nebula
{
	class Bundle;

	// Datastack types
	using TByte = uint8_t;
	using TInt32 = int32_t;
	using TFloat = float_t;
	using TString = std::string;
	using TBundle = std::shared_ptr<Bundle>;

	/// <summary>
	/// Enum for variant lookup and emit
	/// </summary>
	enum DataStackVariantIndex
		: uint8_t {
		_TypeByte	= 0,
		_TypeInt32	= 1,
		_TypeFloat	= 2,
		_TypeString = 3,
		_TypeBundle = 4,
		_TypeVoid, // Does not exist in the stack but is used while compiling code
		_TypeBool, // Does not exist in the stack but is used while compiling code
		_UnknownType,
		_TypeLast,
	};

	using DataStackVariant = std::variant<TByte, TInt32, TFloat, TString, TBundle>;

	DataStackVariantIndex StringToStackValue(const std::string& str);

	inline std::string ToString(const DataStackVariant& var)
	{
		if (std::holds_alternative<TByte>(var))
		{
			return std::to_string(std::get<_TypeByte>(var));
		}

		if (std::holds_alternative<TInt32>(var))
		{
			return std::to_string(std::get<_TypeInt32>(var));
		}

		if (std::holds_alternative<TFloat>(var))
		{
			return std::to_string(std::get<_TypeFloat>(var));
		}

		if (std::holds_alternative<TString>(var))
		{
			return std::get<_TypeString>(var);
		}

		__debugbreak();
		return "";
	}
}