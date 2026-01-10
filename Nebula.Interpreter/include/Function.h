#pragma once

#include <memory>
#include <vector>
#include <string>

#include "LanguageTypes.h"
#include "InstructionDefs.h"

namespace nebula
{
    class Script;

    using VariableList = std::vector<DataStackVariantIndex>;
    using AttributeList = std::vector<VMAttribute>;
    using InstructionArguments = std::vector<DataStackVariant>;
    using FunctionInstruction = std::pair<VMInstruction, InstructionArguments>;
    using FunctionBody = std::vector<FunctionInstruction>;

    // In memory definition of a script function
    class Function
    {
    public:
        Function(const Script* parentScript, DataStackVariantIndex returnType, const std::string& name);

		const Script* GetScript() const { return m_ParentScript; }
        const std::string& Namespace() const;
        const std::string& Name() const { return m_Name; }
        const FunctionBody& Instructions() const { return m_Body; }
        const AttributeList& Attributes() const { return m_Attributes; }
        const VariableList& Parameters() const { return m_Parameters; }
        const VariableList& Locals() const { return m_LocalVariables; }
        DataStackVariantIndex ReturnType() const { return m_ReturnType; }

        bool AddAttribute(VMAttribute attribute);
        bool AddLocalVariable(DataStackVariantIndex type);
        bool AddParameter(DataStackVariantIndex type);
        bool AppendInstruction(const FunctionInstruction& instruction);

    private:
        const Script* m_ParentScript{ nullptr };

        DataStackVariantIndex   m_ReturnType;
        std::string		        m_Name;

        VariableList            m_Parameters;
        AttributeList           m_Attributes;
        VariableList            m_LocalVariables;
        FunctionBody            m_Body;
    };
}

