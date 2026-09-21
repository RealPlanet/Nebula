#pragma once

#include <vector>

#include "LanguageTypes.h"
#include "interfaces/IGCObject.h"
#include "Value.h"

namespace nebula
{
    class VariantArray
        : public IGCObject {
    public:
        VariantArray();

        void Append(const DataStackVariant& v);
        void Append(DataStackVariant&& v);
        void Clear();
        size_t Size();
        Value& At(size_t i);
        const Value& At(size_t i) const;

        virtual InstructionErrorCode CallVirtual(const std::string_view& funcName, nebula::Interpreter* interpreter, Frame* context) override;

    private:
        std::vector<Value> m_Vector;
    };
}

