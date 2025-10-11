#include "VariantArray.h"
#include "Instruction.h"
#include "Frame.h"

using namespace nebula;

nebula::VariantArray::VariantArray(const DataStackVariantIndex& type)
    : m_eVariantType{ type }
{
}

InstructionErrorCode VariantArray::CallVirtual(const std::string_view& funcName, nebula::Interpreter*, Frame* context)
{
    if (funcName == "Append")
    {
        DataStackVariant& v = context->Stack().Peek();

        if (v.index() != this->m_eVariantType)
        {
            return InstructionErrorCode::Fatal;
        }

        Append(v);
        context->Stack().Pop();
        return InstructionErrorCode::None;
    }

    if (funcName == "Clear")
    {
        Clear();
        return InstructionErrorCode::None;
    }

    if (funcName == "Count")
    {
        context->Stack().Push({ (TInt32)Size() });
        return InstructionErrorCode::None;
    }

    return InstructionErrorCode::NativeFunctionNotFound;
}
