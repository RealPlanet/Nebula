#include <cassert>

#include "Function.h"
#include "Script.h"

using namespace nebula;

Function::Function(const Script* parentScript, DataStackVariantIndex returnType, const std::string& name)
    : m_ParentScript{ parentScript }, m_ReturnType{ returnType }, m_Name{ name }
{
    assert(m_ParentScript && "Parent script is null");
    assert(!m_Name.empty() && "Function name is empty");
}

const std::string& nebula::Function::Namespace() const
{
    return m_ParentScript->Namespace();
}

bool Function::AddAttribute(VMAttribute attribute)
{
    m_Attributes.emplace_back(attribute);
    return true;
}

bool nebula::Function::AddLocalVariable(DataStackVariantIndex type)
{
    m_LocalVariables.emplace_back(type);
    return true;
}

bool nebula::Function::AddParameter(DataStackVariantIndex type)
{
    m_Parameters.emplace_back(type);
    return true;
}

bool nebula::Function::AppendInstruction(const FunctionInstruction& instruction)
{
    m_Body.emplace_back(instruction);
    return true;
}

bool nebula::Function::HasAttribute(VMAttribute attr) const
{
    for (int i{ 0 }; i < m_Attributes.size(); i++)
    {
        if (m_Attributes[i] == attr)
            return true;
    }

    return false;
}
