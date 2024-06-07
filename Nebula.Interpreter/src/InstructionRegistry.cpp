#include <cassert>

#include "InstructionRegistry.h"
#include "Frame.h"
#include "DataStack.h"
#include "Bundle.h"
#include "Interpreter.h"

using namespace nebula;

InstructionArguments nebula::GenerateArgumentsForOpcode(VMInstruction opcode, const RawArguments& args)
{
    switch (opcode)
    {
    case VMInstruction::Nop:
    case VMInstruction::Pop:
    case VMInstruction::Dup:
    case VMInstruction::Ret:
    case VMInstruction::Wait:
    case VMInstruction::Add:
    case VMInstruction::Sub:
    case VMInstruction::Mul:
    case VMInstruction::Div:
    case VMInstruction::Rem:
    case VMInstruction::Ceq:
    case VMInstruction::Cgt:
    case VMInstruction::Clt:
    case VMInstruction::And:
    case VMInstruction::Or:
    case VMInstruction::Neg:
    case VMInstruction::Not:
    case VMInstruction::Xor:
    {
        return { /* No arguments */ };
    }
    case VMInstruction::Call_t:
    case VMInstruction::Call:
    case VMInstruction::Ld_b:
    {
        assert(args.size() == 1 || args.size() == 2);

        switch (args.size())
        {
        case 1:
        {
            // FuncName
            return { args[0] };
        }
        case 2:
        {
            // FuncName - Namespace
            return { args[0], args[1] };
        }
        }
        break;
    }
    case VMInstruction::AddStr:     // Number of strings on stack to sum
    {
        assert(args.size() == 1);
        char* p{ nullptr };
        long converted = strtol(args[0].data(), &p, 10);
        assert(p != nullptr);
        return { converted };
    }
    case VMInstruction::StBloc:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::StBArg:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::LdBloc:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::LdBarg:	    // Contains index so we convert it like an i4 constant
    {
        assert(args.size() == 2);
        char* p{ nullptr };
        long converted = strtol(args[0].data(), &p, 10);
        assert(p != nullptr);

        long converted2 = strtol(args[1].data(), &p, 10);
        assert(p != nullptr);

        return { converted, converted2 };
    }
    case VMInstruction::Stloc:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::StArg:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::Ldloc:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::Ldarg:	    // Contains index so we convert it like an i4 constant
    case VMInstruction::BrFalse:    // Contains index so we convert it like an i4 constant
    case VMInstruction::BrTrue:     // Contains index so we convert it like an i4 constant
    case VMInstruction::Br:         // Contains index so we convert it like an i4 constant
    case VMInstruction::Ldc_i4:
    {
        assert(args.size() == 1);
        char* p{ nullptr };
        long converted = strtol(args[0].data(), &p, 10);
        assert(p != nullptr);
        return { converted };
    }
    case VMInstruction::Ldc_i4_0:
    {
        return { 0 };
    }
    case VMInstruction::Ldc_i4_1:
    {
        return { 1 };
    }
    case VMInstruction::Ldc_s:
    {
        assert(args.size() == 1);
        return { args[0] };
    }
    case VMInstruction::ConvType:
    {
        assert(args.size() == 1);
        const std::string& targetType = args[0];
        return { (TInt32)StringToStackValue(targetType) };
    }
    case VMInstruction::LastInstruction:
        __debugbreak(); //  should Not happen
    }


    std::cerr << "Unknown opcode or aguments not valid!\n   " << itos(opcode) << "\n";
    throw std::exception("Unknown opcode or aguments not valid!");
}

InstructionErrorCode nebula::ExecuteInstruction(VMInstruction opcode, Interpreter* interpreter, Frame* context, const InstructionArguments& args)
{
    DataStack& stack = context->Stack();
    switch (opcode)
    {
        // Math
    case VMInstruction::Add:
    {
        assert(stack.Size() >= 2);
        DataStackVariant& a = stack.Peek();
        DataStackVariant& b = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(a));
        assert(std::holds_alternative<TInt32>(b));

        int valA = std::get<DataStackVariantIndex::_TypeInt32>(a);
        stack.Pop();

        int valB = std::get<DataStackVariantIndex::_TypeInt32>(b);
        stack.Pop();

        TInt32 result = valA + valB;
        stack.Push(result);

        return InstructionErrorCode::None;
    }
    case VMInstruction::Sub:
    {
        assert(stack.Size() >= 2);
        DataStackVariant& b = stack.Peek();
        DataStackVariant& a = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(a));
        assert(std::holds_alternative<TInt32>(b));

        TInt32 valA = std::get<DataStackVariantIndex::_TypeInt32>(a);
        stack.Pop();
        TInt32 valB = std::get<DataStackVariantIndex::_TypeInt32>(b);
        stack.Pop();

        TInt32 result = valA - valB;
        stack.Push(result);

        return InstructionErrorCode::None;
    }
    case VMInstruction::Mul:
    {
        assert(stack.Size() >= 2);
        DataStackVariant& b = stack.Peek();
        DataStackVariant& a = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(a));
        assert(std::holds_alternative<TInt32>(b));

        TInt32 valA = std::get<DataStackVariantIndex::_TypeInt32>(a);
        stack.Pop();
        TInt32 valB = std::get<DataStackVariantIndex::_TypeInt32>(b);
        stack.Pop();

        TInt32 result = valA * valB;
        stack.Push(result);

        return InstructionErrorCode::None;
    }
    case VMInstruction::Div:
    {
        assert(stack.Size() >= 2);
        DataStackVariant& b = stack.Peek();
        DataStackVariant& a = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(a));
        assert(std::holds_alternative<TInt32>(b));

        TInt32 div = std::get<DataStackVariantIndex::_TypeInt32>(b);
        stack.Pop();
        if (div == 0)
        {
            return InstructionErrorCode::DivideByZero;
        }

        TInt32 valA = std::get<DataStackVariantIndex::_TypeInt32>(a);
        stack.Pop();

        TInt32 result = valA / div;
        stack.Push(result);

        return InstructionErrorCode::None;
    }
    case VMInstruction::Rem:
    {
        assert(stack.Size() >= 2);
        DataStackVariant& b = stack.Peek();
        DataStackVariant& a = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(a));
        assert(std::holds_alternative<TInt32>(b));

        TInt32 div = std::get<DataStackVariantIndex::_TypeInt32>(b);
        stack.Pop();

        if (div == 0)
        {
            return InstructionErrorCode::DivideByZero;
        }

        TInt32 valA = std::get<DataStackVariantIndex::_TypeInt32>(a);
        stack.Pop();

        TInt32 result = valA % div;
        stack.Push(result);

        return InstructionErrorCode::None;
    }
    // General
    case VMInstruction::Nop:
    {
        return InstructionErrorCode::None;
    }
    case VMInstruction::Pop:
    {
        stack.Pop();
        return InstructionErrorCode::None;
    }
    case VMInstruction::Dup:
    {
        stack.Dup();
        return InstructionErrorCode::None;
    }
    case VMInstruction::Call_t:
    case VMInstruction::Call:
    {
        assert(args.size() == 1 || args.size() == 2);
        assert(std::holds_alternative<TString>(args[0]));

        bool threaded = opcode == VMInstruction::Call_t;
        switch (args.size())
        {
        case 1:
        {
            // Implicit namespace OR builtin function call
            const TString& ns = context->Namespace();
            const TString& funcName = std::get<DataStackVariantIndex::_TypeString>(args[0]);
            const Function* function = interpreter->GetFunction(ns, funcName);

            [[likely]]
            if (function != nullptr)
            {
                interpreter->CreateFrameOnStack(function, threaded);
                return InstructionErrorCode::None;
            }

            /* TODO :: Built in should be able to be thread too! */
            const NativeFunctionCallback* nativeFuncPtr = interpreter->GetNativeFunction(funcName);
            [[unlikely]]
            if (nativeFuncPtr == nullptr)
            {
                // Function not found
                return InstructionErrorCode::NativeFunctionNotFound;
            }

            // Should return instruction error
            auto nativeResult = (*nativeFuncPtr)(interpreter, context);
            assert(nativeResult == InstructionErrorCode::None);
            return nativeResult;
        }
        case 2:
        {
            assert(std::holds_alternative<TString>(args[1]));

            // Namespace is not explicit, function MUST exist
            const TString& ns = std::get<DataStackVariantIndex::_TypeString>(args[0]);
            const TString& funcName = std::get<DataStackVariantIndex::_TypeString>(args[1]);
            auto function = interpreter->GetFunction(ns, funcName);
            if (function != nullptr)
            {
                interpreter->CreateFrameOnStack(function, threaded);
                return InstructionErrorCode::None;
            }

            return InstructionErrorCode::FunctionNotFound;
        }
        }
        break;
    }
    case VMInstruction::Wait:
    {
        TInt32& millis = std::get<TInt32>(stack.Peek());
        context->SetScheduledSleep(((size_t)millis) * 1000);
        stack.Pop();

        return InstructionErrorCode::None;
    }
    case VMInstruction::Ret:
    {
        const Function* func = context->GetFunction();
        if (func->ReturnType() == DataStackVariantIndex::_TypeVoid)
            return InstructionErrorCode::None;

        Frame* parent = context->Parent();
        if (parent != nullptr)
        {
            DataStack& parentStack = parent->Stack();
            assert(stack.Size() == 1);

            DataStackVariant& retValue = stack.Peek();

            [[unlikely]]
            if (retValue.index() != func->ReturnType())
                return InstructionErrorCode::Fatal;

            parentStack.Push(retValue);
            stack.Pop();
        }

        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldc_i4_0:
    {
        stack.Push(0);
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldc_i4_1:
    {
        stack.Push(1);
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldc_i4:
    {
        assert(args.size() == 1);
        assert(std::holds_alternative<TInt32>(args[0]));

        stack.Push(std::get<DataStackVariantIndex::_TypeInt32>(args[0]));
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldc_s:
    {
        assert(args.size() == 1);
        assert(std::holds_alternative<TString>(args[0]));

        const TString& cStr = std::get<DataStackVariantIndex::_TypeString>(args[0]);
        stack.Push(cStr);
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ld_b:
    {
        assert(args.size() == 1 || args.size() == 2);
        const BundleDefinition* bundleDefinition = nullptr;
        switch (args.size())
        {
        case 1:
        {
            const TString& bundleTypeName = std::get<DataStackVariantIndex::_TypeString>(args[0]);
            bundleDefinition = interpreter->GetBundleDefinition(context->Namespace(), bundleTypeName);
            break;
        }
        case 2:
        {
            const TString& namespaceName = std::get<DataStackVariantIndex::_TypeString>(args[0]);
            const TString& bundleTypeName = std::get<DataStackVariantIndex::_TypeString>(args[1]);
            bundleDefinition = interpreter->GetBundleDefinition(namespaceName, bundleTypeName);
            break;
        }
        default:
        {
            return InstructionErrorCode::Fatal;
        }
        }

        if (bundleDefinition == nullptr)
        {
            return InstructionErrorCode::BundleNotFound;
        }

        stack.Push(Bundle::FromDefinition(*bundleDefinition));
        return InstructionErrorCode::None;
    }
    case VMInstruction::LdBloc:
    {
        assert(args.size() == 2);
        assert(std::holds_alternative<TInt32>(args[0]));
        assert(std::holds_alternative<TInt32>(args[1]));

        TInt32 localIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        TInt32 bundleIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[1]);
        FrameVariable& var = context->Memory().LocalAt(localIndex);

        TBundle bundle = std::get<DataStackVariantIndex::_TypeBundle>(var.Value());
        stack.Push(bundle->Get(bundleIndex));

        return InstructionErrorCode::None;
    }
    case VMInstruction::LdBarg:
    {
        assert(args.size() == 2);
        assert(std::holds_alternative<TInt32>(args[0]));
        assert(std::holds_alternative<TInt32>(args[1]));

        TInt32 paramIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        TInt32 bundleIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[1]);
        FrameVariable& var = context->Memory().ParamAt(paramIndex);

        TBundle bundle = std::get<DataStackVariantIndex::_TypeBundle>(var.Value());
        stack.Push(bundle->Get(bundleIndex));
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldloc:
    {
        assert(args.size() == 1);
        assert(std::holds_alternative<TInt32>(args[0]));

        TInt32 localIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        FrameVariable& var = context->Memory().LocalAt(localIndex);

        stack.Push(var.Value());
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ldarg:
    {
        assert(args.size() == 1);
        assert(std::holds_alternative<TInt32>(args[0]));

        TInt32 argIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        FrameVariable& var = context->Memory().ParamAt(argIndex);

        stack.Push(var.Value());
        return InstructionErrorCode::None;

    }
    case VMInstruction::AddStr:
    {
        TInt32 numOfStrings = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        // TODO :: manually sum strings up to 4, then use more expensive way!

        std::vector<std::string> strs;
        strs.reserve((size_t)numOfStrings);
        for (TInt32 i{ 0 }; i < numOfStrings; i++)
        {
            DataStackVariant strVal = context->Stack().Peek();
            context->Stack().Pop();
            assert(std::holds_alternative<TString>(strVal));
            std::string newStr = std::get<DataStackVariantIndex::_TypeString>(strVal);
            strs.push_back(newStr);
        }

        std::string finalString;
        for (TInt32 i{ numOfStrings - 1 }; i >= 0; i--)
        {
            finalString += strs[i];
        }

        context->Stack().Push({ finalString });
        return InstructionErrorCode::None;
    }
    case VMInstruction::StBloc:
    {
        TInt32 localIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        TInt32 bundleIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[1]);

        FrameVariable& var = context->Memory().LocalAt(localIndex);
        DataStackVariant value = stack.Peek();
        stack.Pop();

        TBundle bundlePtr = std::get<DataStackVariantIndex::_TypeBundle>(var.Value());
        if (bundlePtr->SetAt(bundleIndex, value))
        {
            return InstructionErrorCode::None;
        }

        // Types don't match
        return InstructionErrorCode::Fatal;
    }
    case VMInstruction::StBArg:
    {
        TInt32 localIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        TInt32 bundleIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[1]);

        FrameVariable& var = context->Memory().ParamAt(localIndex);
        DataStackVariant value = stack.Peek();
        stack.Pop();

        TBundle bundlePtr = std::get<DataStackVariantIndex::_TypeBundle>(var.Value());
        if (bundlePtr->SetAt(bundleIndex, value))
        {
            return InstructionErrorCode::None;
        }

        // Types don't match
        return InstructionErrorCode::Fatal;
    }
    case VMInstruction::Stloc:
    {
        TInt32 localIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        FrameVariable& var = context->Memory().LocalAt(localIndex);
        DataStackVariant value = stack.Peek();
        stack.Pop();

        if (var.SetValue(value))
        {
            return InstructionErrorCode::None;
        }
        // Types don't match
        return InstructionErrorCode::Fatal;
    }
    case VMInstruction::StArg:
    {
        TInt32 argIndex = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        FrameVariable& var = context->Memory().ParamAt(argIndex);
        DataStackVariant value = stack.Peek();
        stack.Pop();

        if (var.SetValue(value))
        {
            return InstructionErrorCode::None;
        }
        // Types don't match
        return InstructionErrorCode::Fatal;
    }
    case VMInstruction::Xor:
    {
        DataStackVariant& v1 = stack.Peek();
        DataStackVariant& v2 = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(v1));
        assert(std::holds_alternative<TInt32>(v2));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(v1);
        stack.Pop();

        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(v2);
        stack.Pop();

        stack.Push(val1 ^ val2);
        break;
    }
    case VMInstruction::Neg: // -
    {
        DataStackVariant& v = stack.Peek();

        if (std::holds_alternative<TInt32>(v))
        {
            TInt32 i = std::get<DataStackVariantIndex::_TypeInt32>(v);
            stack.Pop();
            stack.Push(-i);
            return InstructionErrorCode::None;
        }

        if (std::holds_alternative<TFloat>(v))
        {
            TFloat f = std::get<DataStackVariantIndex::_TypeFloat>(v);
            stack.Pop();
            stack.Push(-f);
            return InstructionErrorCode::None;
        }

        return InstructionErrorCode::Fatal;
    }
    case VMInstruction::Not: // ~ Only ints
    {
        DataStackVariant& v = stack.Peek();
        assert(std::holds_alternative<TInt32>(v));
        TInt32 i = std::get<DataStackVariantIndex::_TypeInt32>(v);
        stack.Pop();
        stack.Push(~i);
        return InstructionErrorCode::None;
    }
    // Control flow
    case VMInstruction::Clt:
    {
        DataStackVariant& second = stack.Peek();
        DataStackVariant& first = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(first));
        assert(std::holds_alternative<TInt32>(second));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(first);
        stack.Pop();

        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(second);
        stack.Pop();

        if (val1 < val2)
            stack.Push(1);
        else
            stack.Push(0);

        return InstructionErrorCode::None;
    }
    case VMInstruction::Cgt:
    {
        DataStackVariant& second = stack.Peek();
        DataStackVariant& first = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(first));
        assert(std::holds_alternative<TInt32>(second));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(first);
        stack.Pop();
        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(second);
        stack.Pop();

        if (val1 > val2)
            stack.Push(1);
        else
            stack.Push(0);

        return InstructionErrorCode::None;
    }
    case VMInstruction::And:
    {
        DataStackVariant& second = stack.Peek();
        DataStackVariant& first = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(first));
        assert(std::holds_alternative<TInt32>(second));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(first);
        stack.Pop();
        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(second);
        stack.Pop();

        stack.Push((int)(val1 && val2));
        return InstructionErrorCode::None;
    }
    case VMInstruction::Or:
    {
        DataStackVariant& second = stack.Peek();
        DataStackVariant& first = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(first));
        assert(std::holds_alternative<TInt32>(second));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(first);
        stack.Pop();
        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(second);
        stack.Pop();

        stack.Push((int)(val1 || val2));
        return InstructionErrorCode::None;
    }
    case VMInstruction::Ceq:
    {
        DataStackVariant& second = stack.Peek();
        DataStackVariant& first = stack.Peek(1);

        assert(std::holds_alternative<TInt32>(first));
        assert(std::holds_alternative<TInt32>(second));

        TInt32 val1 = std::get<DataStackVariantIndex::_TypeInt32>(first);
        stack.Pop();
        TInt32 val2 = std::get<DataStackVariantIndex::_TypeInt32>(second);
        stack.Pop();

        if (val1 == val2)
            stack.Push(1);
        else
            stack.Push(0);

        return InstructionErrorCode::None;
    }
    case VMInstruction::BrFalse:
    {
        // Assert target label number
        DataStackVariant& val = stack.Peek();
        assert(std::holds_alternative<TInt32>(val));
        TInt32 i = std::get<DataStackVariantIndex::_TypeInt32>(val);
        stack.Pop();

        if (i == 0)
        {
            assert(std::holds_alternative<TInt32>(args[0]));
            TInt32 jmpLabel = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);

            context->SetNextInstruction(jmpLabel);
        }

        return InstructionErrorCode::None;
    }
    case VMInstruction::BrTrue:
    {
        // Assert target label number
        DataStackVariant& val = stack.Peek();
        assert(std::holds_alternative<TInt32>(val));
        TInt32 i = std::get<DataStackVariantIndex::_TypeInt32>(val);
        stack.Pop();

        if (i == 1)
        {
            assert(std::holds_alternative<TInt32>(args[0]));
            TInt32 jmpLabel = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);

            context->SetNextInstruction(jmpLabel);
        }

        return InstructionErrorCode::None;
    }
    case VMInstruction::Br:
    {
        // Assert target label number
        assert(std::holds_alternative<TInt32>(args[0]));
        TInt32 jmpLabel = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        context->SetNextInstruction(jmpLabel);
        return InstructionErrorCode::None;
    }
    case VMInstruction::ConvType:
    {
        assert(std::holds_alternative<TInt32>(args[0]));
        TInt32 targetType = std::get<DataStackVariantIndex::_TypeInt32>(args[0]);
        DataStackVariant& prevValue = stack.Peek();

        switch (targetType)
        {
        case DataStackVariantIndex::_TypeString:
        {
            TString value = ToString(prevValue);
            stack.Pop();
            stack.Push(value);
            break;
        }
        default:
            __debugbreak(); //  should Not happen in normal operation
            return InstructionErrorCode::Fatal;
        }

        return InstructionErrorCode::None;
    }
    case VMInstruction::LastInstruction:
        __debugbreak(); //  should Not happen
    }

    std::cerr << "Unknown opcode or aguments not valid during execution!\n   " << itos(opcode) << "\n";
    throw std::exception("Unknown opcode or aguments not valid during execution");
}
