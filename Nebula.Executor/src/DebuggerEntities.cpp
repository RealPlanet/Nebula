#include "DebuggerEntities.h"
#include "DebuggerUtility.h"

#include <format>

bool nebula::debugger::Value::CanDebuggerChangeValue() const
{
	if (!typeInformation)
	{
		return false;
	}

	return typeInformation->kind != symbols::TypeInformation::eKind::Object &&
		typeInformation->kind != symbols::TypeInformation::eKind::Array;
}

bool nebula::debugger::Value::OverrideValue(const std::string& newValue, std::string& failReason)
{
	if (!typeInformation)
	{
		return false;
	}

	if (!CanDebuggerChangeValue())
	{
		return false;
	}

	if (typeInformation->name == "bool")
	{
		return OverrideBoolValue(newValue, failReason);
	}

	if (typeInformation->name == "int32")
	{
		return OverrideInt32Value(newValue, failReason);
	}

	if (typeInformation->name == "float")
	{
		return OverrideFloatValue(newValue, failReason);
	}

	if (typeInformation->name == "string")
	{
		return OverrideStringValue(newValue, failReason);
	}


	return false;
}

std::string nebula::debugger::Value::GetDisplayValue() const
{
	if (!typeInformation)
	{
		// TODO Technically we can infer from runtime information SOME value
		return "No type information is available";
	}

	if (typeInformation->kind == symbols::TypeInformation::eKind::Object)
	{
		if (internalValue->ContainsGCObject())
		{
			return "{ ... }";
		}

		return "undefined object";
	}

	if (typeInformation->kind == symbols::TypeInformation::eKind::Array)
	{
		if (internalValue->ContainsGCObject())
		{
			return "[ ... ]";
		}

		return "undefined array";
	}

	if (typeInformation->name == "bool")
	{
		return std::to_string(internalValue->AsInt32() != 0);
	}

	if (typeInformation->name == "string")
	{
		return "\"" + nebula::ToString(internalValue->GetInternalValue()) + "\"";
	}

	return nebula::ToString(internalValue->GetInternalValue());
}

bool nebula::debugger::Value::OverrideBoolValue(const std::string& newValue, std::string& failReason)
{
	auto loweredString = utility::to_lower(newValue);
	if (loweredString == "true")
	{
		return internalValue->SetValue({ true });
	}

	if (loweredString == "false")
	{
		return internalValue->SetValue({ false });
	}

	size_t value;
	if (debugger::utility::try_parse(loweredString, value))
	{
		return internalValue->SetValue({ value != 0 });
	}

	failReason = std::format("Value '{}' is not convertible to type '{}'", newValue, typeInformation->name);
	return false;
}

bool nebula::debugger::Value::OverrideInt32Value(const std::string& newValue, std::string& failReason)
{
	size_t value;
	if (utility::try_parse(newValue, value))
	{
		return internalValue->SetValue({ value != 0 });
	}

	failReason = std::format("Value '{}' is not convertible to type '{}'", newValue, typeInformation->name);
	return false;
}

bool nebula::debugger::Value::OverrideFloatValue(const std::string& newValue, std::string& failReason)
{
	double value;
	if (utility::try_parse(newValue, value))
	{
		return internalValue->SetValue({ (float)(value) });
	}

	failReason = std::format("Value '{}' is not convertible to type '{}'", newValue, typeInformation->name);
	return false;
}

bool nebula::debugger::Value::OverrideStringValue(const std::string& newValue, std::string&)
{
	return internalValue->SetValue({ newValue });
}
