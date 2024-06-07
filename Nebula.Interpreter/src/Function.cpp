#include "Function.h"
#include "Script.h"

using namespace nebula;

Function::Function(const Script* parentScript, DataStackVariantIndex returnType, const std::string& name)
    : m_ParentScript{ parentScript }, m_ReturnType{ returnType }, m_Name{ name }
{
    assert(m_ParentScript);
    assert(!m_Name.empty());
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
