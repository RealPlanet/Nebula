#include "Bundle.h"

using namespace nebula;

BundleField::BundleField(const std::string& name, DataStackVariantIndex fieldType)
    : m_Name{ name }, m_AcceptedType{ fieldType }, m_Value{}
{

}

bool BundleField::SetValue(DataStackVariant& value, bool allowTypeMismatch)
{
    if (allowTypeMismatch)
    {
        m_Value = value;
        m_AcceptedType = (DataStackVariantIndex)m_Value.index();
        return true;
    }

    // AcceptedTypes is always set, m_Value instead is left as default initially
    if (m_AcceptedType != value.index())
    {
        return false;
    }

    m_Value = value;
    return true;
}

BundleDefinition::BundleDefinition(const std::string& name)
    : m_BundleName{ name }, m_Fields{}
{
}

void BundleDefinition::AddField(const BundleFieldDefinition& field)
{
    m_Fields.emplace_back(field);
}

std::shared_ptr<Bundle> Bundle::FromDefinition(const BundleDefinition& definition)
{
    // Constructor is private! Cant use make_shared
    Bundle* b = new Bundle();
    std::shared_ptr<Bundle> result = std::shared_ptr<Bundle>(b);

    result->m_Name = definition.Name();
    for (auto it = definition.Fields().begin(); it != definition.Fields().cend(); it++) {
        result->m_Fields.emplace_back(it->first, it->second);
    }

    return result;
}

DataStackVariant& Bundle::Get(int index)
{
    BundleField& f = m_Fields[index];
    return f.FieldValue();
}

DataStackVariant& Bundle::GetByName(const std::string& name)
{
    for (auto f : m_Fields)
    {
        if (f.Name() == name)
        {
            return f.FieldValue();
        }
    }

    throw std::exception("Error");
}

bool Bundle::SetAt(int index, DataStackVariant& data)
{
    return m_Fields[index].SetValue(data);
}
