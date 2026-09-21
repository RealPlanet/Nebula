#include "Bundle.h"

using namespace nebula;

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
		result->m_Fields.emplace_back(it->second);
	}

	return result;
}

DataStackVariant& Bundle::Get(int index)
{
	auto& f = m_Fields[index];
	return f.GetInternalValue();
}

Value& nebula::Bundle::GetVariable(int index)
{
	auto& f = m_Fields[index];
	return f;
}

bool Bundle::SetAt(int index, DataStackVariant& data)
{
	return m_Fields[index].SetValue(data);
}

nebula::Bundle::Bundle()
	: IGCObject(ObjectType::Bundle)
{
}
