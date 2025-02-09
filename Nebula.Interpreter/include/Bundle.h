#pragma once

#include <map>
#include <vector>

#include "LanguageTypes.h"
#include "interfaces/AwaitableObject.h"
#include "interfaces/AllocatedObject.h"

namespace nebula
{
    // Definition of a bundle field as imported from a script
    using BundleFieldDefinition = std::pair<std::string, DataStackVariantIndex>;

    using BundleFields = std::vector<std::pair<std::string, DataStackVariantIndex>>;

    // Definition of a bundle as imported from a script
    class BundleDefinition
    {
    public:
        BundleDefinition(const std::string& name);

        const std::string_view Name() const { return m_BundleName; }
        const BundleFields& Fields() const { return m_Fields; }

        void AddField(const BundleFieldDefinition& field);

    private:
        std::string m_BundleName;
        BundleFields m_Fields;
    };

    // In memory rapresentation of a bundle field
    class BundleField
    {
    public:
        BundleField(const std::string& name, DataStackVariantIndex fieldType);

        const std::string& Name() { return m_Name; }
        DataStackVariantIndex FieldType() const { return m_AcceptedType; };
        const DataStackVariant& FieldValue() const { return m_Value; }
        DataStackVariant& FieldValue() { return m_Value; }

        bool SetValue(DataStackVariant&, bool allowTypeMismatch = false);

    private:
        std::string				m_Name;
        DataStackVariant		m_Value;
        DataStackVariantIndex	m_AcceptedType{ _UnknownType };
    };

    // In memory rapresentation of a bundle
    class Bundle
        : public AllocatedObject, public AwaitableObject
    {
    public:
        static std::shared_ptr<Bundle> FromDefinition(const BundleDefinition& definition);

    public:
        const std::string& Name() { return m_Name; }
        size_t FieldCount() const { return m_Fields.size(); }

        DataStackVariant& Get(int index);
        DataStackVariant& GetByName(const std::string& name);

        bool SetAt(int index, DataStackVariant& data);
        void ClearFields() { m_Fields.clear(); }

    private:
        Bundle() = default;
        std::string m_Name;
        std::vector<BundleField> m_Fields;
    };
}

