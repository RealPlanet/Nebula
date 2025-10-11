#pragma once

#include <memory>
#include <variant>
#include <string>
#include <vector>

namespace nebula
{
    template<typename TType>
    using RefCounted = std::shared_ptr<TType>;

    class Bundle;
    class VariantArray;
    class IGCObject;

    // Datastack types
    //using TByte = uint8_t;
    using TInt32 = int32_t;
    using TFloat = float_t;
    using TString = std::string;
    using TBundle = RefCounted<Bundle>;
    using TArray = RefCounted<VariantArray>;
    using TGCObject = RefCounted<IGCObject>;

    /// <summary> Enum for variant lookup and emit </summary>
    enum DataStackVariantIndex
        : uint8_t {
        _TypeInt32 = 0,
        _TypeFloat,
        _TypeString,
        _TypeBundle,
        _TypeArray,
        _TypeVoid, // Does not exist in the stack but is used while compiling code
        //_TypeBool, // Does not exist in the stack but is used while compiling code
        _UnknownType,
        _TypeLast,
    };

    //using DataStackVariant = std::variant<TByte, TInt32, TFloat, TString, TBundle>;
    using DataStackVariant = std::variant<TInt32, TFloat, TString, TBundle, TArray>;

    DataStackVariantIndex StringToStackValue(const std::string& str);

    inline std::string ToString(const DataStackVariant& var)
    {
        //if (std::holds_alternative<TByte>(var))
        //{
        //	return std::to_string(std::get<_TypeByte>(var));
        //}

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