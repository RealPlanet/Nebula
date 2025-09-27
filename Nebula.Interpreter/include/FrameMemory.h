#pragma once

#include "LanguageTypes.h"

namespace nebula
{
    class FrameVariable
    {
        // Used for first time init
        friend class Frame;
    public:
        DataStackVariantIndex	Type() const { return _type; }
        bool					SetValue(DataStackVariant& val);
        DataStackVariant&       Value() { return _value; }
        const DataStackVariant& Value() const { return _value; }

        TInt32					AsInt32() const { return std::get<DataStackVariantIndex::_TypeInt32>(_value); }
        TFloat					AsFloat() const { return std::get<DataStackVariantIndex::_TypeFloat>(_value); }
        const TString&          AsString() const { return std::get<DataStackVariantIndex::_TypeString>(_value); }
        const TBundle*          AsBundle() const { return std::get_if<TBundle>(&_value); }
        const TArray*           AsArray() const { return std::get_if<TArray>(&_value); }
        const TGCObject*        AsGCObject() const;

    private:
        DataStackVariantIndex _type{ DataStackVariantIndex::_UnknownType };
        DataStackVariant _value;
    };

    class FrameMemory
    {
    public:
        FrameMemory(size_t paramCount, size_t localCount);
        FrameMemory(FrameMemory&& f) = delete;
        FrameMemory(const FrameMemory&) = delete; // No copy allowed
        ~FrameMemory();

        FrameVariable& LocalAt(size_t i);
        FrameVariable& ParamAt(size_t i);
        const FrameVariable& LocalAt(size_t i) const;
        const FrameVariable& ParamAt(size_t i) const;

        size_t ParamCount() const { return m_ParamCount; }
        size_t LocalCount() const { return m_LocalCount; }

    private:
        FrameVariable* m_Variables{ nullptr };
        size_t m_ParamCount{ 0 };
        size_t m_LocalCount{ 0 };
    };
}

