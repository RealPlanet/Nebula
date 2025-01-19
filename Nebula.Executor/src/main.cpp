#define _CRTDBG_MAP_ALLOC

#include <stdlib.h>
#include <crtdbg.h>

#include <iostream>
#include <memory>
#include <vector>

#include <format>
#include <variant>
#include <chrono>
#include <iostream>
#include <thread>

// Interpreter
#include "Interpreter.h"
#include "DataStack.h"

#include "ConsoleWriter.h"

using namespace nebula::frontend;
using namespace nebula;

static void PrintReport(shared::DiagnosticReport& report)
{
    for (auto& err : report.Errors())
    {
        std::string  msg = "[ERROR] :: " + err.Message();
        writer::ConsoleWrite(msg.data(), writer::BG_RED);
    }
}

static void BindNativeFunctions(Interpreter& vm)
{
    vm.BindNativeFunction("Write", [](Interpreter*, Frame* context) {
        DataStackVariant& var = context->Stack().Peek();

        const std::string& arg = nebula::ToString(var);
        std::cout << arg;
        context->Stack().Pop();
        return nebula::InstructionErrorCode::None;
        });

    vm.BindNativeFunction("WriteLine", [](Interpreter*, Frame* context) {
        DataStackVariant& messageVariant = context->Stack().Peek();
        const std::string& arg = nebula::ToString(messageVariant);
        std::cout << arg << "\n";
        context->Stack().Pop();
        return nebula::InstructionErrorCode::None;
        });

    vm.BindNativeFunction("HashString", [](Interpreter*, Frame* context) {
        static std::hash<TString> h;

        DataStackVariant& messageVariant = context->Stack().Peek();

        std::string arg = nebula::ToString(messageVariant);
        const TInt32 stringHash = (TInt32)h(arg);
        context->Stack().Pop();
        context->Stack().Push(stringHash);
        return nebula::InstructionErrorCode::None;
        });
}

static int PrintVMLastError(nebula::Interpreter& vm)
{
    if (vm.GetState() == Interpreter::State::Abort)
    {
        writer::ConsoleWrite("VM was aborted, stack trace is:", writer::FG_RED);

        shared::ErrorCallStack* callstack = vm.GetFatalErrorCallstack();
        writer::ConsoleWrite(callstack->GetAsText().data(), writer::FG_RED);

        return (int)callstack->GetErrorCode();
    }
    else
    {
        writer::ConsoleWrite("[ No VM errors detected ]", writer::FG_BLUE);
    }

    return 0;
}

static int ExecuteVM(std::vector<std::string>& scripts) {

    // Setup interpreter and bind native functions
    // Might be useful to have them available even if we haven't started the VM (Load time caching ecc..)
    Interpreter vm;
    BindNativeFunctions(vm);

    std::vector<std::shared_ptr<Script>> loadedScripts;
    loadedScripts.reserve(scripts.size());

    for (auto& file : scripts)
    {
        ScriptLoadResult scriptLoadResult = Script::FromFile(file);

        if (scriptLoadResult.ParsingReport.Errors().size() > 0)
        {
            std::string errMessage = std::format("Errors while loading script {}", file.data());
            writer::ConsoleWrite(errMessage, writer::Code::FG_RED);
            PrintReport(scriptLoadResult.ParsingReport);
            return -2;
        }

        PrintReport(scriptLoadResult.ParsingReport);
        writer::ConsoleWrite(std::format("Script with namespace {} has been loaded", scriptLoadResult.Script->Namespace()), writer::Code::FG_GREEN);
        loadedScripts.push_back(scriptLoadResult.Script);
    }

    auto start = std::chrono::high_resolution_clock::now();

    for (auto& ptr : loadedScripts)
    {
        bool scriptLoaded = vm.AddScript(ptr);
        [[unlikely]]
        if (!scriptLoaded)
        {
            std::string errMessage = std::format("Error while adding script '{}' to VM", ptr->Namespace().data());
            writer::ConsoleWrite(errMessage, writer::Code::FG_RED);
            return -99999;
        }
    }

    auto startNoScriptLoading = std::chrono::high_resolution_clock::now();

    std::cout << ">------- Begin execution ---------\n";

    vm.InitAndRun();
    vm.Wait();

    auto finish = std::chrono::high_resolution_clock::now();
    auto milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(finish - start);
    auto microseconds = std::chrono::duration_cast<std::chrono::microseconds>(finish - start);

    auto millisecondsNoLoading = std::chrono::duration_cast<std::chrono::milliseconds>(finish - startNoScriptLoading);
    auto microsecondsNoLoading = std::chrono::duration_cast<std::chrono::microseconds>(finish - startNoScriptLoading);

    std::cout << ">---------------------------------\n";
    std::cout << "Execution terminated with time:" << std::endl;
    std::cout << "  (With script loading) ->   " << milliseconds.count() << "ms; " << microseconds.count() << "us\n";
    std::cout << "  (No script loading) ->   " << millisecondsNoLoading.count() << "ms; " << microsecondsNoLoading.count() << "us\n";

    return PrintVMLastError(vm);
}

int main(int argc, char* argv[]) {
#ifdef _DEBUG
    // Enable memory anal
    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
    _CrtSetReportMode(_CRT_WARN, _CRTDBG_MODE_DEBUG);

#endif // DEBUG


    std::vector<std::string> scriptPaths;
    scriptPaths.reserve(argc);
    for (int i = 1; i < argc; i++)
        scriptPaths.push_back(argv[i]);

    /* DO NOT DUMP MEMORY HERE OTHERWISE STATIC STUFF WILL BE REPORTED */
    return ExecuteVM(scriptPaths);
}