#include "DebugTypes.h"
#include "Utility.h"

using namespace nebula::debugger;

bool DebugVariable::OverrideValue(const std::string& valueToset, std::string& reason)
{
	if (debugInformation->internalType == "bundle" ||
		debugInformation->internalType == "array")
	{
		// TODO Allow well defined structures
		return false;
	}

	if (debugInformation->sourceType == "bool")
	{
		auto lStr = utility::to_lower(valueToset);
		if (lStr == "true")
		{
			originalVariable->SetValue({ true });
			return true;
		}

		if (lStr == "false")
		{
			originalVariable->SetValue({ false });
			return true;
		}

		size_t value;
		if (!utility::try_parse(valueToset, value))
		{
			reason = std::format("Value '{}' is not convertible to type '{}'", valueToset, debugInformation->sourceType);
			return false;
		}

		originalVariable->SetValue({ value != 0 });
		return true;
	}

	if (debugInformation->internalType == "int32")
	{
		size_t value;
		if (!utility::try_parse(valueToset, value))
		{
			reason = std::format("Value '{}' is not convertible to type '{}'", valueToset, debugInformation->sourceType);
			return false;
		}

		originalVariable->SetValue({ value != 0 });
		return true;
	}

	if (debugInformation->internalType == "float")
	{
		double value;
		if (!utility::try_parse(valueToset, value))
		{
			reason = std::format("Value '{}' is not convertible to type '{}'", valueToset, debugInformation->sourceType);
			return false;
		}

		originalVariable->SetValue({ (float)(value)});
		return true;
	}

	if (debugInformation->internalType == "string")
	{
		originalVariable->SetValue({ valueToset});
		return true;
	}

	return false;
}

std::string DebugVariable::GetDisplayValue() const
{
	if (debugInformation->sourceType == "bool")
	{
		if (originalVariable->AsInt32() != 0)
		{
			return "true";
		}

		return "false";
	}

	if (debugInformation->internalType == "bundle")
	{
		if (originalVariable->ContainsGCObject())
		{
			return "{ ... }";
		}

		return "undefined";
	}

	if (debugInformation->internalType == "array")
	{
		if (originalVariable->ContainsGCObject())
		{
			return "[ ... ]";
		}

		return "undefined";
	}

	return nebula::ToString(originalVariable->Value());
}

std::string DebugVariable::GetDisplayType() const
{
	if (debugInformation->sourceType == "bool")
	{
		return "bool";
	}

	if (debugInformation->internalType == "bundle")
	{
		return std::format("{}::{}", debugInformation->sourceNamespace, debugInformation->sourceType);
	}

	if (debugInformation->internalType == "array")
	{
		if (debugInformation->sourceNamespace != "")
		{
			return std::format("{}::{}[]", debugInformation->sourceNamespace, debugInformation->sourceType);
		}
		else
		{
			return std::format("{}[]", debugInformation->sourceType);
		}
	}

	return debugInformation->internalType;
}
