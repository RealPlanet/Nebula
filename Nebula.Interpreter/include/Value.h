#pragma once

#include "LanguageTypes.h"

namespace nebula
{
	class Value
	{
	public:
		Value() { _type = _UnknownType; }
		Value(DataStackVariantIndex initialType) { _type = initialType; }
		Value(const DataStackVariant&);
		Value(DataStackVariant&&);

		DataStackVariantIndex	GetValueType() const { return _type; }
		bool					SetValue(const DataStackVariant& val);
		DataStackVariant&		GetInternalValue() { return _value; }
		const DataStackVariant& GetInternalValue() const { return _value; }

		TInt32					AsInt32() const { return std::get<DataStackVariantIndex::_TypeInt32>(_value); }
		TFloat					AsFloat() const { return std::get<DataStackVariantIndex::_TypeFloat>(_value); }
		const TString&			AsString() const { return std::get<DataStackVariantIndex::_TypeString>(_value); }
		const TGCObject&		AsGCObject() const { return std::get<DataStackVariantIndex::_TypeObject>(_value); }

		bool					ContainsGCObject() { return _type == DataStackVariantIndex::_TypeObject && _value.index() == _type; }

	private:
		DataStackVariantIndex _type{ DataStackVariantIndex::_UnknownType };
		DataStackVariant _value;
	};
}
