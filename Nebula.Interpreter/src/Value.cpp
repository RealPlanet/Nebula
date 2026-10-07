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
	if (val.index() != _type)
	{
		if (val.index() == DataStackVariantIndex::_TypeInt32 &&
			_type == DataStackVariantIndex::_TypeBool)
		{
			_value = std::get<DataStackVariantIndex::_TypeInt32>(val) != 0;
			return true;
		}

		return false;
	}

	_value = val;
	return true;
}

void Value::Initialize(DataStackVariantIndex type)
{
	if (_type != DataStackVariantIndex::_UnknownType)
	{
		assert(false);
		return;
	}

	_type = type;
}
