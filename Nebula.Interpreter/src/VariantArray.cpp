#include "VariantArray.h"
#include "Instruction.h"
#include "Frame.h"

#include <cassert>

using namespace nebula;

VariantArray::VariantArray()
    : GCUser(ObjectType::Array)
{
}

void VariantArray::Append(const DataStackVariant& v)
{
    m_Vector.emplace_back(v);
}

void nebula::VariantArray::Append(DataStackVariant&& v)
{
    m_Vector.emplace_back(std::move(v));
}

void VariantArray::Clear() { m_Vector.clear(); }

size_t VariantArray::Size() { return m_Vector.size(); }

Value& nebula::VariantArray::At(size_t i)
{
    return m_Vector.at(i);
}

const Value& nebula::VariantArray::At(size_t i) const
{
    return m_Vector.at(i);
}

InstructionErrorCode VariantArray::CallVirtual(const std::string_view& funcName, std::vector<DataStackVariant>& arguments, Interpreter*, Frame* context)
{
    if (funcName == "Append")
    {
        if (arguments.size() != 1)
        {
            return InstructionErrorCode::Fatal;
        }

        DataStackVariant& v = arguments[0];
        if (!m_Vector.empty() && m_Vector[0].GetValueType() != v.index())
        {
            return InstructionErrorCode::Fatal;
        }

        Append(std::move(v));
        return InstructionErrorCode::None;
    }

    if (funcName == "Clear")
    {
        assert(arguments.size() == 0);
        Clear();
        return InstructionErrorCode::None;
    }

    if (funcName == "Count")
    {
        assert(arguments.size() == 0);
        context->Stack().Push({ (TInt32)Size() });
        return InstructionErrorCode::None;
    }

    return InstructionErrorCode::NativeFunctionNotFound;
}
