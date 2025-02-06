#include <map>

#include "LanguageTypes.h"


using namespace nebula;

DataStackVariantIndex nebula::StringToStackValue(const std::string& str)
{
	static std::map<std::string, DataStackVariantIndex> valMap = {
		//{ "char",	DataStackVariantIndex::_TypeByte },
		{ "bool",	DataStackVariantIndex::_TypeInt32 },
		{ "int32",	DataStackVariantIndex::_TypeInt32 },
		{ "float",	DataStackVariantIndex::_TypeFloat },
		{ "string", DataStackVariantIndex::_TypeString },
		{ "void",	DataStackVariantIndex::_TypeVoid },
		{ "bundle", DataStackVariantIndex::_TypeBundle },
	};

	auto it = valMap.find(str);
	if (it == valMap.end())
	{
		return DataStackVariantIndex::_UnknownType;
	}

	return it->second;
}
