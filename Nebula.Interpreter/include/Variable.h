#pragma once

#include "LanguageTypes.h"

namespace nebula
{
	class Variable
	{
		friend class Frame;
	public:
		Variable() { _type = _UnknownType; }
		Variable(DataStackVariantIndex initialType) { _type = initialType; }

		DataStackVariantIndex	Type() const { return _type; }
		bool					SetValue(DataStackVariant& val);
		DataStackVariant&		Value() { return _value; }
		const DataStackVariant& Value() const { return _value; }

		TInt32					AsInt32() const { return std::get<DataStackVariantIndex::_TypeInt32>(_value); }
		TFloat					AsFloat() const { return std::get<DataStackVariantIndex::_TypeFloat>(_value); }
		const TString&			AsString() const { return std::get<DataStackVariantIndex::_TypeString>(_value); }
		const TGCObject&		AsGCObject() const { return std::get<DataStackVariantIndex::_TypeObject>(_value); }

	private:
		DataStackVariantIndex _type{ DataStackVariantIndex::_UnknownType };
		DataStackVariant _value;
	};
}