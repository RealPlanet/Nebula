#pragma once
#include "NebulaExports.h"

#include "interfaces/IStreamWrapper.h"

#include "Interpreter.h"
#include "CallStack.h"
#include "Frame.h"
#include "Script.h"
#include "DebuggerDefinitions.h"



class StdRedirector
    : public nebula::IStreamWrapper {
private:
    nebula::interop::StringFuncPtr _writeCallback;
    nebula::interop::StringFuncPtr _writeLineCallback;
public:

    StdRedirector(nebula::interop::StringFuncPtr writeLineCallback, nebula::interop::StringFuncPtr writeCallback) {
        _writeCallback = writeCallback;
        _writeLineCallback = writeLineCallback;
    }

    virtual void WriteLine(const std::string& line) {
        _writeLineCallback(line.data());
    }

    virtual void Write(const std::string& line) {
        _writeCallback(line.data());
    }
};

static void WriteReportToCallback(const char* scriptPath, nebula::interop::ReportCallbackPtr ptr, nebula::shared::DiagnosticReport& report)
{
    auto& errors = report.Errors();
    size_t size = errors.size();

    for (int i{ 0 }; i < size; i++)
    {
        auto& report = errors[i];
        ptr(scriptPath, nebula::shared::ReportType::Error, report.Message().data());
    }

    auto& warnings = report.Warnings();
    size = warnings.size();

    for (int i{ 0 }; i < size; i++)
    {
        auto& report = warnings[i];
        ptr(scriptPath, nebula::shared::ReportType::Warning, report.Message().data());
    }

    auto& informations = report.Information();
    size = informations.size();

    for (int i{ 0 }; i < size; i++)
    {
        auto& report = informations[i];
        ptr(scriptPath, nebula::shared::ReportType::Information, report.Message().data());
    }
}

// Used for loadlibrary, TODO :: Figure out a better approch for non windows systems
#include <windows.h>

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      GENERAL
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

void General_DestroyIntArray(int* handle)
{
    delete[] handle;
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      INTERPRETER
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

nebula::Interpreter* Interpreter_Create()
{
    return new nebula::Interpreter();
}

bool Interpreter_LoadSpecificBindingsInDLL(nebula::Interpreter* handle, const char* dllLibrary, const char** functionNames, int arrLen)
{
    if (handle == nullptr)
    {
        return false;
    }

    HINSTANCE hGetProcIDDLL = LoadLibraryA(dllLibrary);
    if (!hGetProcIDDLL) {
        return false;
    }

    const char* procGetBindingName = NEB_GET_NATIVE_BINDING_NAME;
    nebula::NativeFunctionCallbackPtr(*funcPtr)(const char*) = (NEB_GET_BINDING_PTR)GetProcAddress(hGetProcIDDLL, procGetBindingName);
    bool result = false;
    if (funcPtr != nullptr)
    {
        for (int i{ 0 }; i < arrLen; i++)
        {
            const char* funName = functionNames[i];

            nebula::NativeFunctionCallbackPtr bindingPtr = funcPtr(funName);

            // If we find it we add it!
            if (bindingPtr != nullptr)
            {
                handle->BindNativeFunction(funName, *bindingPtr);
            }
        }

        result = true;
    }

    return result;
}

bool Interpreter_LoadBindingsInDLL(nebula::Interpreter* handle, const char* dllLibrary)
{
    if (handle == nullptr)
    {
        return false;
    }

    HINSTANCE hGetProcIDDLL = LoadLibraryA(dllLibrary);
    if (!hGetProcIDDLL) {
        return false;
    }

    NEB_GET_ALL_BINDINGS_PTR(funcPtr) = (NEB_GET_ALL_BINDINGS_PTR_TYPE)GetProcAddress(hGetProcIDDLL, NEB_GET_ALL_NATIVE_BINDINGS_NAME);
    auto& bindings = *funcPtr();
    for (auto& kvp : bindings)
    {
        std::string bindingName{ kvp.first.data() };
        if (!handle->BindNativeFunction(bindingName, kvp.second))
        {
            return false;
        }
    }

    return true;
}

bool Interpreter_RedirectOutput(nebula::Interpreter* handle, nebula::interop::StringFuncPtr writeCb, nebula::interop::StringFuncPtr writeLineCb)
{
    if (handle == nullptr)
    {
        return false;
    }

    StdRedirector* ptr = new StdRedirector(writeLineCb, writeCb);
    return handle->SetStandardOutput(ptr);
}

bool Interpreter_RedirectExitCallback(nebula::Interpreter* handle, nebula::interop::ExitFuncPtr callback)
{
    if (handle == nullptr)
    {
        return false;
    }

    return handle->SetExitCallback(callback);
}

bool Interpreter_ClearRedirectOutput(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return false;
    }

    return handle->ClearStandardOutput();
}

bool Interpreter_AddScripts(nebula::Interpreter* handle, nebula::interop::ReportCallbackPtr callbackPtr, const char** scriptPaths, int arrLen)
{
    if (handle == nullptr)
    {
        return false;
    }

    for (int i{ 0 }; i < arrLen; i++)
    {
        const char* str = scriptPaths[i];

        nebula::ScriptLoadResult loadResult = nebula::Script::FromFile(str);

        WriteReportToCallback(str, callbackPtr, loadResult.ParsingReport);

        size_t errCount = loadResult.ParsingReport.Errors().size();
        if (errCount > 0)
        {
            return false;
        }

        std::shared_ptr<nebula::Script> sharedPtr{ loadResult.Script };
        bool addedToVm = handle->AddScript(sharedPtr);

        if (!addedToVm)
        {
            //_logger->LogError(System::String::Format("Could not add script '{0}' to native interpreter", scriptName));
            return false;
        }
    }

    //_logger->LogInformation(System::String::Format("Loaded '{0}' script into virtual machine", scriptPaths->Count));
    return true;
}

int* Interpreter_GetNextOpcodeForAllThreads(nebula::Interpreter* handle, int* arrLen)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    auto& threads = handle->GetThreadMap();

    size_t size = threads.Count();
    int* allOpcodes = new int[size];

    for (int i{ 0 }; i < size; i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
        {
            allOpcodes[i] = -1;
            continue;
        }

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        allOpcodes[i] = static_cast<int>(frame->NextInstructionIndex());
    }

    return allOpcodes;
}

void Interpreter_Init(nebula::Interpreter* handle, bool startPaused)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Init(startPaused);
}

void Interpreter_Run(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Run();
}

void Interpreter_Step(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Step();
}

void Interpreter_Pause(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Pause();
}

void Interpreter_Stop(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Stop();
}

void Interpreter_Reset(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return;
    }

    handle->Reset();
}

long Interpreter_GetCurrentThreadId(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    return (long)handle->GetCurrentThreadId();
}

long Interpreter_GetThreadCount(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    return (int)handle->GetThreadMap().Count();
}

int Interpreter_GetState(nebula::Interpreter* handle)
{
    if (handle == nullptr)
    {
        return nebula::Interpreter::State::Abort;
    }

    return handle->GetState();
}

int Interpreter_AnyFrameJustStarted(nebula::Interpreter* handle, const char* ns, const char* funcName)
{
    if (handle == nullptr)
    {
        return -1;
    }

    const nebula::ThreadMap& threads = handle->GetThreadMap();

    for (int i = 0; i < threads.Count(); i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
            continue;

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        // We need JUST started
        if (frame->NextInstructionIndex() != 0)
            continue;

        const nebula::Function* func = frame->GetFunction();
        if (func->Namespace() != ns)
            continue;

        if (func->Name() != funcName)
            continue;

        return i;
    }

    return -1;
}

int Interpreter_AnyFrameAt(nebula::Interpreter* handle, const char* ns, const char* funcName, int nextOpcodeIndex)
{
    if (handle == nullptr)
    {
        return -1;
    }

    const nebula::ThreadMap& threads = handle->GetThreadMap();

    for (int i = 0; i < threads.Count(); i++)
    {
        const nebula::CallStack& stack = threads.At(i);
        if (stack.size() == 0)
            continue;

        const nebula::Frame* frame = stack.at(stack.size() - 1);
        if (frame->NextInstructionIndex() != nextOpcodeIndex)
            continue;

        const nebula::Function* func = frame->GetFunction();

        if (func->Namespace() != ns)
            continue;

        if (func->Name() != funcName)
            continue;

        return i;
    }

    return -1;
}

int Interpreter_GetCurrentOpcodeIndexOfThread(nebula::Interpreter* handle, int threadId)
{
    if (handle == nullptr)
    {
        return -1;
    }

    const nebula::ThreadMap& threads = handle->GetThreadMap();

    if (threadId < 0 || threadId >= threads.Count())
    {
        return -1;
    }

    const nebula::CallStack& stack = threads.At(threadId);
    if (stack.size() == 0)
    {
        return -1;
    }

    const nebula::Frame* frame = stack.at(stack.size() - 1);
    size_t index = frame->NextInstructionIndex();
    return (int)index;
}

const nebula::CallStack* Interpreter_GetCallStackOfThread(nebula::Interpreter* handle, int threadId)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    auto& threadMap = handle->GetThreadMap();
    if (threadId < 0 || threadId >= threadMap.Count())
    {
        return nullptr;
    }
    const nebula::CallStack* callStack = &threadMap.At(threadId);
    return callStack;
}

void Interpreter_Destroy(nebula::Interpreter* handle)
{
    delete handle;
}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      FRAME
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

const char* Frame_GetFunctionName(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return handle->GetFunction()->Name().data();
}

const char* Frame_GetFunctionNamespace(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return handle->GetFunction()->Namespace().data();
}

int Frame_GetNextInstructionIndex(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    size_t index = handle->NextInstructionIndex();
    return static_cast<int>(index);
}

int Frame_GetLocalCount(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    size_t count = handle->Memory().LocalCount();
    return static_cast<int>(count);
}

int Frame_GetParameterCount(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    if (handle == nullptr)
    {
        return -1;
    }

    size_t count = handle->Memory().ParamCount();
    return static_cast<int>(count);
}

int Frame_GetInstructionCount(nebula::Frame* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    size_t count = handle->Memory().ParamCount();
    return static_cast<int>(count);
}

nebula::FrameVariable* Frame_GetLocalVariableAt(nebula::Frame* handle, int index)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    if (index < 0 || index >= Frame_GetLocalCount(handle))
    {
        return nullptr;
    }

    return &handle->Memory().LocalAt(index);
}

nebula::FrameVariable* Frame_GetParameterVariableAt(nebula::Frame* handle, int index)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    if (index < 0 || index >= Frame_GetParameterCount(handle))
    {
        return nullptr;
    }

    return &handle->Memory().ParamAt(index);
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      CALLSTACK
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////


int CallStack_GetFrameCount(nebula::CallStack* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return (int)handle->size();
}

nebula::Frame* CallStack_GetFrameAt(nebula::CallStack* handle, int index)
{
    if (handle == nullptr || index < 0 || index >= handle->size())
    {
        return nullptr;
    }

    return handle->at(index);
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      SCRIPT
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

nebula::Script* Script_FromFile(const char* path, nebula::interop::ReportCallbackPtr reportCallback)
{
    nebula::ScriptLoadResult result = nebula::Script::FromFile(std::string{ path });

    // TODO :: Figure out how to return report

    nebula::Script* ptr = result.Script;

    WriteReportToCallback(path, reportCallback, result.ParsingReport);

    return ptr;
}

const char* Script_GetNamespace(nebula::Script* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    nebula::Script* script = ((nebula::Script*)handle);

    return script->Namespace().data();
}

Functions Script_GetFunctions(nebula::Script* handle, int* outCount)
{
    *outCount = -1;
    if (handle == nullptr)
    {
        return nullptr;
    }

    auto& functions = handle->Functions();
    auto count = functions.size();
    Functions pointers = new const nebula::Function * [count];

    int i{ 0 };
    for (auto it = functions.begin(); it != functions.end(); it++, i++) {
        auto ptr = &(it->second);
        pointers[i] = ptr;
    }

    *outCount = (int)functions.size();
    return pointers;
}

void Script_DestroyFunctionList(Functions handle)
{
    delete[] handle;
}

BundleDefinitions Script_GetBundleDefinitions(nebula::Script* handle, int* outCount)
{
    *outCount = -1;
    if (handle == nullptr)
    {
        return nullptr;
    }

    auto& bundles = handle->Bundles();

    int bundleCount = (int)bundles.size();

    BundleDefinitions pointers = new const nebula::BundleDefinition * [bundleCount];

    int i{ 0 };
    for (auto it = bundles.begin(); it != bundles.end(); it++, i++) {
        pointers[i] = &it->second;
    }

    *outCount = (int)bundles.size();
    return pointers;
}

void Script_DestroyBundleDefinitionList(BundleDefinitions handle)
{
    delete[] handle;
}

void Script_Destroy(nebula::Script* handle)
{
    delete (nebula::Script*)handle;
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      BUNDLE DEFINITION
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

const char* BundleDefinition_GetName(nebula::BundleDefinition* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return handle->Name().data();
}

const nebula::BundleFieldDefinition** BundleDefinition_GetFields(nebula::BundleDefinition* handle, int* arrLen)
{
    if (handle == nullptr)
    {
        *arrLen = 0;
        return nullptr;
    }

    auto& fields = handle->Fields();
    *arrLen = (int)fields.size();
    const nebula::BundleFieldDefinition** array = new const nebula::BundleFieldDefinition * [*arrLen];
    for (int i{ 0 }; i < *arrLen; i++)
    {
        array[i] = &fields[i];
    }

    return array;
}

void BundleDefinition_DestroyFieldDefinitionsList(const nebula::BundleFieldDefinition** handle)
{
    delete[] handle;
}

void BundleDefinition_Destroy(nebula::BundleDefinition* handle)
{
    delete (nebula::BundleDefinition*)handle;
}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      BUNDLE
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////


int Bundle_GetFieldCount(nebula::Bundle* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return handle->FieldCount();
}

nebula::DataStackVariant* Bundle_GetField(nebula::Bundle* handle, int index)
{
    return nullptr;
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      DATA STACK VARIANT
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////


int DataStackVariant_GetType(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return handle->index();
}

const char* DataStackVariant_GetStringValue(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }
    return std::get<nebula::DataStackVariantIndex::_TypeString>(*handle).data();
}

int DataStackVariant_GetIntValue(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return std::get<nebula::DataStackVariantIndex::_TypeInt32>(*handle);
}

float DataStackVariant_GetFloatValue(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return std::get<nebula::DataStackVariantIndex::_TypeFloat>(*handle);
}

nebula::TBundle* DataStackVariant_GetBundleValue(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return std::get_if<nebula::TBundle>(handle);
}

nebula::TArray* DataStackVariant_GetArrayValue(nebula::DataStackVariant* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return std::get_if<nebula::TArray>(handle);
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      BUNDLE FIELD
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

const char* BundleField_GetName(nebula::BundleFieldDefinition* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    return handle->first.data();
}

int BundleField_GetType(nebula::BundleFieldDefinition* handle)
{
    if (handle == nullptr)
    {
        return 0;
    }

    return handle->second;
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      FUNCTION
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////


const char* Function_GetName(nebula::Function* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    nebula::Function* obj = handle;
    return obj->Name().data();
}

const char* Function_GetNamespace(nebula::Function* handle)
{
    if (handle == nullptr)
    {
        return nullptr;
    }

    nebula::Function* obj = handle;
    return obj->Namespace().data();
}

int Function_GetReturnType(nebula::Function* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    nebula::Function* obj = handle;
    return obj->ReturnType();
}

int* Function_GetAttributes(nebula::Function* handle, int* arrLen)
{
    if (handle == nullptr)
    {
        *arrLen = 0;
        return nullptr;
    }

    auto& attributes = handle->Attributes();
    *arrLen = (int)attributes.size();
    int* resArray = new int[*arrLen];
    for (int i{ 0 }; i < *arrLen; i++)
    {
        resArray[i] = (int)attributes.at(i);
    }

    return resArray;
}

int* Function_GetParameters(nebula::Function* handle, int* arrLen)
{
    if (handle == nullptr)
    {
        *arrLen = 0;
        return nullptr;
    }

    auto& parameters = handle->Parameters();
    *arrLen = (int)parameters.size();
    int* resArray = new int[*arrLen];
    for (int i{ 0 }; i < *arrLen; i++)
    {
        resArray[i] = (int)parameters.at(i);
    }

    return resArray;
}

int* Function_GetInstructions(nebula::Function* handle, int* arrLen)
{
    if (handle == nullptr)
    {
        *arrLen = 0;
        return nullptr;
    }

    auto& instructions = handle->Instructions();
    *arrLen = (int)instructions.size();
    int* resArray = new int[*arrLen];
    for (int i{ 0 }; i < *arrLen; i++)
    {
        resArray[i] = (int)instructions.at(i).first;
    }

    return resArray;
}

void Function_Destroy(nebula::Function* handle)
{
    delete (nebula::Function*)handle;
}

/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
//      FRAME VARIABLE
/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

int FrameVariable_GetType(nebula::FrameVariable* handle)
{
    if (handle == nullptr)
    {
        return -1;
    }

    return (int)handle->Type();
}

/*In some init cases type is set but no value is present. For example when breaking as soon as we enter in a function scope*/
#define CHECK_FRAME_VAR_INIT(val)\
if (handle->Value().index() != handle->Type())\
{\
    return val;\
}

const char* FrameVariable_GetStringValue(nebula::FrameVariable* handle)
{
    if (handle == nullptr || handle->Type() != nebula::DataStackVariantIndex::_TypeString)
    {
        return nullptr;
    }

    CHECK_FRAME_VAR_INIT(nullptr);

    const nebula::TString& string = handle->AsString();
    return string.data();
}

bool FrameVariable_SetStringValue(nebula::FrameVariable* handle, const char* value)
{
    if (handle == nullptr)
    {
        return false;
    }

    nebula::DataStackVariant variant{ std::string(value) };
    return handle->SetValue(variant);
}

int FrameVariable_GetIntValue(nebula::FrameVariable* handle)
{
    if (handle == nullptr || handle->Type() != nebula::DataStackVariantIndex::_TypeInt32)
    {
        return 0;
    }

    CHECK_FRAME_VAR_INIT(0);

    return handle->AsInt32();
}

bool FrameVariable_SetIntValue(nebula::FrameVariable* handle, int value)
{
    if (handle == nullptr)
    {
        return false;
    }

    nebula::DataStackVariant variant{ value };
    return handle->SetValue(variant);
}

float FrameVariable_GetFloatValue(nebula::FrameVariable* handle)
{
    if (handle == nullptr || handle->Type() != nebula::DataStackVariantIndex::_TypeFloat)
    {
        return 0;
    }

    CHECK_FRAME_VAR_INIT(0);

    return handle->AsFloat();
}

bool FrameVariable_SetFloatValue(nebula::FrameVariable* handle, float value)
{
    if (handle == nullptr)
    {
        return false;
    }

    nebula::DataStackVariant variant{ value };
    return handle->SetValue(variant);
}

nebula::Bundle* FrameVariable_GetBundleValue(nebula::FrameVariable* handle)
{
    if (handle == nullptr || handle->Type() != nebula::DataStackVariantIndex::_TypeBundle)
    {
        return nullptr;
    }

    CHECK_FRAME_VAR_INIT(nullptr);

    return handle->AsBundle()->get();
}

nebula::VariantArray* FrameVariable_GetArrayValue(nebula::FrameVariable* handle)
{
    if (handle == nullptr || handle->Type() != nebula::DataStackVariantIndex::_TypeArray)
    {
        return nullptr;
    }

    CHECK_FRAME_VAR_INIT(nullptr);

    return handle->AsArray()->get();
}
