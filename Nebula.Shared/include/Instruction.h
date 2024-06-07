#pragma once

#include "LanguageTypes.h"
#include "InstructionDefs.h"

namespace nebula
{
    using RawArguments = std::vector<std::string>;
    using InstructionArguments = std::vector<DataStackVariant>;

    enum class InstructionErrorCode
    {
        Unknown,
        None,
        Fatal,
        FunctionNotFound,
        NativeFunctionNotFound,
        BundleNotFound,
        DivideByZero,
    };

    inline std::string InstructionErrorCodeToString(InstructionErrorCode c)
    {
        switch (c)
        {
        case InstructionErrorCode::Unknown:
            return "Unknown";
        case InstructionErrorCode::None:
            return "None";
        case InstructionErrorCode::Fatal:
            return "Fatal";
        case InstructionErrorCode::FunctionNotFound:
            return "FunctionNotFound";
        case InstructionErrorCode::NativeFunctionNotFound:
            return "NativeFunctionNotFound";
        case InstructionErrorCode::BundleNotFound:
            return "BundleNotFound";
        case InstructionErrorCode::DivideByZero:
            return "DivideByZero";
        }

        return "Unknown error code: " + (int)c;
    }
}