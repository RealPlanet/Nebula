#include "Value.h"
#include "LanguageTypes.h"

#include <cassert>
#include <utility>

using namespace nebula;

Value::Value(const DataStackVariant& other)
	: _value{ other }, _type{ (nebula::DataStackVariantIndex)other.index() }
{
}

Value::Value(DataStackVariant&& other)
	: _value{ std::move(other) }, _type{ (nebula::DataStackVariantIndex)other.index() }
{
}

bool Value::SetValue(const DataStackVariant& val)
{
	if (_type != nebula::DataStackVariantIndex ::_UnknownType && _type != val.index())
	{
		assert(false);
		return false;
	}

	_value = val;
	return true;
}
