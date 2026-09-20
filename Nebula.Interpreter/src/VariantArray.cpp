#include "VariantArray.h"
#include "Instruction.h"
#include "Frame.h"

using namespace nebula;

VariantArray::VariantArray()
    : IGCObject(ObjectType::Array)
{
}

void VariantArray::Append(const DataStackVariant& v)
{
    m_Vector.emplace_back(v);
}

void VariantArray::Clear() { m_Vector.clear(); }

size_t VariantArray::Size() { return m_Vector.size(); }

DataStackVariant& VariantArray::operator[](int i) { return m_Vector[i]; }

InstructionErrorCode VariantArray::CallVirtual(const std::string_view& funcName, Interpreter*, Frame* context)
{
    if (funcName == "Append")
    {
        DataStackVariant& v = context->Stack().Peek();

        if (!m_Vector.empty() && m_Vector[0].index() != v.index())
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
