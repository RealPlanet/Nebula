#pragma once

#include <vector>

#include "LanguageTypes.h"
#include "Value.h"
#include "interfaces/IGCObject.h"

namespace nebula
{
    // Definition of a bundle field as imported from a script
    using BundleFieldDefinition = std::pair<std::string, DataStackVariantIndex>;

    using BundleFields = std::vector<BundleFieldDefinition>;

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

    // In memory rapresentation of a bundle
    class Bundle
        : public IGCObject
    {
    public:
        static std::shared_ptr<Bundle> FromDefinition(const BundleDefinition& definition);

    public:
        const std::string& Name() { return m_Name; }
        size_t FieldCount() const { return m_Fields.size(); }

        DataStackVariant& Get(int index);
        Value& GetVariable(int index);

        bool SetAt(int index, DataStackVariant& data);
        void ClearFields() { m_Fields.clear(); }

    private:
        Bundle();
        std::string m_Name;
        std::vector<Value> m_Fields;
    };
}

