#pragma once

#include "LanguageTypes.h"

namespace Nebula::CodeEmitter::Types
{
    public enum class TypeIdentifier
    {
        Unknown = nebula::DataStackVariantIndex::_UnknownType,
        Void = nebula::DataStackVariantIndex::_TypeVoid,
        Char = nebula::DataStackVariantIndex::_TypeByte,
        Bool = nebula::DataStackVariantIndex::_TypeBool,
        Int32 = nebula::DataStackVariantIndex::_TypeInt32,
        Float = nebula::DataStackVariantIndex::_TypeFloat,
        String = nebula::DataStackVariantIndex::_TypeString,
        Bundle = nebula::DataStackVariantIndex::_TypeBundle,
    };
}