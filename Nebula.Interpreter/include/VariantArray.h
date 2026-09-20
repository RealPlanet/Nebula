#pragma once

#include <map>
#include <vector>

#include "LanguageTypes.h"
#include "interfaces/IGCObject.h"

namespace nebula
{
    class VariantArray
        : public IGCObject {
    public:
        VariantArray();

        void Append(const DataStackVariant& v);
        void Clear();
        size_t Size();
        DataStackVariant& operator[](int i);

        virtual InstructionErrorCode CallVirtual(const std::string_view& funcName, nebula::Interpreter* interpreter, Frame* context) override;

    private:
        // TODO Store object type to ensure consistency
        std::vector<DataStackVariant> m_Vector;
    };
}

