#pragma once

#include <memory>
#include <variant>
#include <string>
#include <vector>

#include "interfaces/IGCObject.h"

namespace nebula
{
    template<typename TType>
    using RefCounted = std::shared_ptr<TType>;



    class Bundle;
    class VariantArray;

    // Datastack types
    //using TByte = uint8_t;
    using TInt32 = int32_t;
    using TFloat = float_t;
    using TString = std::string;
    using TBundle = RefCounted<Bundle>;
    using TArray = RefCounted<VariantArray>;

    /// <summary>
    /// Enum for variant lookup and emit
    /// </summary>
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

    class VariantArray {
    public:
        void append(const DataStackVariant& v)
        {
            if (m_eVariantType != _UnknownType &&
                m_eVariantType != v.index())
            {
                throw std::exception("Variant type differs");
            }

            m_Vector.emplace_back(v);
        }

        void clear() { m_Vector.clear(); }
        size_t size() { return m_Vector.size(); }
        DataStackVariant& operator[](int i) { return m_Vector[i]; }

    private:
        DataStackVariantIndex m_eVariantType{ DataStackVariantIndex::_UnknownType };
        std::vector<DataStackVariant> m_Vector;
    };
}