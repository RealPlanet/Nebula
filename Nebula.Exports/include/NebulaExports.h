#pragma once

#include <vector>
#include <string>
#include "LanguageTypes.h"

namespace nebula {
    class Interpreter;
    class Script;
    class BundleField;
    class BundleDefinition;
    class Bundle;
    class VariantArray;
    class Function;
    class FrameVariable;
    class Frame;

    using CallStack = std::vector<nebula::Frame*>;
    using BundleFieldDefinition = std::pair<std::string, DataStackVariantIndex>;
    using BundleFields = std::vector<BundleFieldDefinition>;

    namespace shared {
        enum class ReportType;
    }
}

namespace nebula {
    namespace interop {

        using StringFuncPtr = void(*)(const char*);
        using ExitFuncPtr = void(*)();
        using ReportCallbackPtr = void(*)(const char* /*scriptName*/, nebula::shared::ReportType, const char*);
    }
}

extern "C"
{
    typedef const nebula::Function** Functions;
    // A list of definition pointers
    typedef const nebula::BundleDefinition** BundleDefinitions;


    // Generic
    __declspec(dllexport) void General_DestroyIntArray(int* handle);

    // Virtual machine
    __declspec(dllexport) nebula::Interpreter* Interpreter_Create();
    __declspec(dllexport) bool Interpreter_LoadSpecificBindingsInDLL(nebula::Interpreter* handle, const char* dllLibrary, const char** functionNames, int arrLen);
    __declspec(dllexport) bool Interpreter_LoadBindingsInDLL(nebula::Interpreter* handle, const char* dllLibrary);
    __declspec(dllexport) bool Interpreter_RedirectOutput(nebula::Interpreter* handle, nebula::interop::StringFuncPtr writeCb, nebula::interop::StringFuncPtr writeLineCb);
    __declspec(dllexport) bool Interpreter_RedirectExitCallback(nebula::Interpreter* handle, nebula::interop::ExitFuncPtr callback);
    __declspec(dllexport) bool Interpreter_ClearRedirectOutput(nebula::Interpreter* handle);
    __declspec(dllexport) bool Interpreter_AddScripts(nebula::Interpreter* handle, nebula::interop::ReportCallbackPtr callbackPtr, const char** scriptPaths, int arrLen);
    __declspec(dllexport) int* Interpreter_GetNextOpcodeForAllThreads(nebula::Interpreter* handle, int* arrLen);
    __declspec(dllexport) void Interpreter_Init(nebula::Interpreter* handle, bool startPaused);
    __declspec(dllexport) void Interpreter_Run(nebula::Interpreter* handle);
    __declspec(dllexport) void Interpreter_Step(nebula::Interpreter* handle);
    __declspec(dllexport) void Interpreter_Pause(nebula::Interpreter* handle);
    __declspec(dllexport) void Interpreter_Stop(nebula::Interpreter* handle);
    __declspec(dllexport) void Interpreter_Reset(nebula::Interpreter* handle);
    __declspec(dllexport) long Interpreter_GetCurrentThreadId(nebula::Interpreter* handle);
    __declspec(dllexport) long Interpreter_GetThreadCount(nebula::Interpreter* handle);
    __declspec(dllexport) int Interpreter_GetState(nebula::Interpreter* handle);
    __declspec(dllexport) int Interpreter_AnyFrameJustStarted(nebula::Interpreter* handle, const char* ns, const char* funcName);
    __declspec(dllexport) int Interpreter_AnyFrameAt(nebula::Interpreter* handle, const char* ns, const char* funcName, int nextOpcodeIndex);
    __declspec(dllexport) int Interpreter_GetCurrentOpcodeIndexOfThread(nebula::Interpreter* handle, int threadId);
    __declspec(dllexport) const nebula::CallStack* Interpreter_GetCallStackOfThread(nebula::Interpreter* handle, int threadId);
    __declspec(dllexport) void Interpreter_Destroy(nebula::Interpreter* handle);

    // Frame
    __declspec(dllexport) const char* Frame_GetFunctionName(nebula::Frame* handle);
    __declspec(dllexport) const char* Frame_GetFunctionNamespace(nebula::Frame* handle);
    __declspec(dllexport) int Frame_GetNextInstructionIndex(nebula::Frame* handle);
    __declspec(dllexport) int Frame_GetLocalCount(nebula::Frame* handle);
    __declspec(dllexport) int Frame_GetParameterCount(nebula::Frame* handle);
    __declspec(dllexport) int Frame_GetInstructionCount(nebula::Frame* handle);
    __declspec(dllexport) nebula::FrameVariable* Frame_GetLocalVariableAt(nebula::Frame* handle, int index);
    __declspec(dllexport) nebula::FrameVariable* Frame_GetParameterVariableAt(nebula::Frame* handle, int index);

    // Callstack
    __declspec(dllexport) int CallStack_GetFrameCount(nebula::CallStack* handle);
    __declspec(dllexport) nebula::Frame* CallStack_GetFrameAt(nebula::CallStack* handle, int index);

    // Script
    __declspec(dllexport) nebula::Script* Script_FromFile(const char* path, nebula::interop::ReportCallbackPtr reportCallback);
    __declspec(dllexport) const char* Script_GetNamespace(nebula::Script* handle);
    __declspec(dllexport) Functions Script_GetFunctions(nebula::Script* handle, int* outCount);
    __declspec(dllexport) void Script_DestroyFunctionList(Functions handle); // Destroy list, not elements
    __declspec(dllexport) BundleDefinitions Script_GetBundleDefinitions(nebula::Script* handle, int* outCount);
    __declspec(dllexport) void Script_DestroyBundleDefinitionList(BundleDefinitions handle); // Destroy list, not elements
    __declspec(dllexport) void Script_Destroy(nebula::Script* handle);

    // BundleDefinition
    __declspec(dllexport) const char* BundleDefinition_GetName(nebula::BundleDefinition* handle);
    __declspec(dllexport) const nebula::BundleFieldDefinition** BundleDefinition_GetFields(nebula::BundleDefinition* handle, int* arrLen);

    // Destroy list, not elements
    __declspec(dllexport) void BundleDefinition_DestroyFieldDefinitionsList(const nebula::BundleFieldDefinition** handle);
    __declspec(dllexport) void BundleDefinition_Destroy(nebula::BundleDefinition* handle);

    // Bundle
    __declspec(dllexport) int Bundle_GetFieldCount(nebula::Bundle* handle);
    __declspec(dllexport) nebula::DataStackVariant* Bundle_GetField(nebula::Bundle* handle, int index);

    // DataStackVariant
    __declspec(dllexport) int DataStackVariant_GetType(nebula::DataStackVariant* handle);
    __declspec(dllexport) const char* DataStackVariant_GetStringValue(nebula::DataStackVariant* handle);
    __declspec(dllexport) int DataStackVariant_GetIntValue(nebula::DataStackVariant* handle);
    __declspec(dllexport) float DataStackVariant_GetFloatValue(nebula::DataStackVariant* handle);
    __declspec(dllexport) nebula::Bundle* DataStackVariant_GetBundleValue(nebula::DataStackVariant* handle);
    __declspec(dllexport) nebula::VariantArray* DataStackVariant_GetArrayValue(nebula::DataStackVariant* handle);

    // BundleField
    __declspec(dllexport) const char* BundleField_GetName(nebula::BundleFieldDefinition* handle);
    __declspec(dllexport) int BundleField_GetType(nebula::BundleFieldDefinition* handle);

    // Function
    __declspec(dllexport) const char* Function_GetName(nebula::Function* handle);
    __declspec(dllexport) const char* Function_GetNamespace(nebula::Function* handle);
    __declspec(dllexport) int Function_GetReturnType(nebula::Function* handle);
    __declspec(dllexport) int* Function_GetAttributes(nebula::Function* handle, int* arrLen);
    __declspec(dllexport) int* Function_GetParameters(nebula::Function* handle, int* arrLen);
    __declspec(dllexport) int* Function_GetInstructions(nebula::Function* handle, int* arrLen);
    __declspec(dllexport) void Function_Destroy(nebula::Function* handle);

    // Variable
    __declspec(dllexport) int FrameVariable_GetType(nebula::FrameVariable* handle);
    __declspec(dllexport) const char* FrameVariable_GetStringValue(nebula::FrameVariable* handle);
    __declspec(dllexport) bool FrameVariable_SetStringValue(nebula::FrameVariable* handle, const char* value);
    __declspec(dllexport) int FrameVariable_GetIntValue(nebula::FrameVariable* handle);
    __declspec(dllexport) bool FrameVariable_SetIntValue(nebula::FrameVariable* handle, int value);
    __declspec(dllexport) float FrameVariable_GetFloatValue(nebula::FrameVariable* handle);
    __declspec(dllexport) bool FrameVariable_SetFloatValue(nebula::FrameVariable* handle, float value);
    __declspec(dllexport) nebula::Bundle* FrameVariable_GetBundleValue(nebula::FrameVariable* handle);
    __declspec(dllexport) nebula::VariantArray* FrameVariable_GetArrayValue(nebula::FrameVariable* handle);
}
