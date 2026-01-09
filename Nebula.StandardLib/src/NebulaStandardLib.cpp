#include <type_traits>
#include <map>
#include <chrono>
#include <string>
#include <string_view>

#include "NebulaStandardLib.h"
#include "DebuggerDefinitions.h"
#include "Interpreter.h"
#include "Frame.h"
#include "Instruction.h"    
#include "LanguageTypes.h"    

using namespace nebula;

static std::map<std::string_view, nebula::NativeFunctionCallbackPtr> bindings = {
    { "HashString", nebula::stdlib::HashString },
    { "WriteLine", nebula::stdlib::WriteLine },
    { "Write", nebula::stdlib::Write },
    { "GetCurrentTimeMillis", nebula::stdlib::GetCurrentTimeMillis },
};

NEB_DECLARE_GET_BINDING(bindingName)
{
    std::string_view key{ bindingName };
    auto it = bindings.find(key);
    if (it != bindings.end())
        return it->second;

    return nullptr;
}

NEB_DECLARE_GET_ALL_BINDINGS
{
    return &bindings;
}

nebula::InstructionErrorCode nebula::stdlib::HashString(Interpreter*, Frame* context)
{
    static std::hash<TString> h;

    DataStackVariant& messageVariant = context->Stack().Peek();

    std::string arg = nebula::ToString(messageVariant);
    const TInt32 stringHash = (TInt32)h(arg);
    context->Stack().Pop();
    context->Stack().Push(stringHash);
    return nebula::InstructionErrorCode::None;
}

nebula::InstructionErrorCode nebula::stdlib::GetCurrentTimeMillis(nebula::Interpreter*, nebula::Frame* ctx)
{
    using namespace std::chrono;
    constexpr sys_days epoch = 2020y / 1 / 1;
    auto now = floor<seconds>(system_clock::now());
    TInt32 timeFrom2020 = static_cast<int>((now - epoch) / 1ms);
    ctx->Stack().Push({ timeFrom2020 });
    return InstructionErrorCode::None;
}

nebula::InstructionErrorCode nebula::stdlib::WriteLine(Interpreter* i, Frame* context)
{
    DataStackVariant& messageVariant = context->Stack().Peek();
    const std::string& arg = nebula::ToString(messageVariant);
    i->StandardOutput()->WriteLine(arg);
    context->Stack().Pop();
    return nebula::InstructionErrorCode::None;
}

nebula::InstructionErrorCode nebula::stdlib::Write(Interpreter* i, Frame* context)
{
    DataStackVariant& messageVariant = context->Stack().Peek();
    const std::string& arg = nebula::ToString(messageVariant);
    i->StandardOutput()->Write(arg);
    context->Stack().Pop();
    return nebula::InstructionErrorCode::None;
}
