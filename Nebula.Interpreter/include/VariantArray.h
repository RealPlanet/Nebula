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
        VariantArray(const DataStackVariantIndex& type);

        void Append(const DataStackVariant& v)
        {
            if (m_eVariantType != _UnknownType &&
                m_eVariantType != v.index())
            {
                throw std::exception("Variant type differs");
            }

            m_Vector.emplace_back(v);
        }

        void Clear() { m_Vector.clear(); }
        size_t Size() { return m_Vector.size(); }
        DataStackVariant& operator[](int i) { return m_Vector[i]; }

        virtual InstructionErrorCode CallVirtual(const std::string_view& funcName, nebula::Interpreter* interpreter, Frame* context) override;

    private:
        DataStackVariantIndex m_eVariantType{ DataStackVariantIndex::_UnknownType };

        // TODO Store object type to ensure consistency

        std::vector<DataStackVariant> m_Vector;
    };
}

