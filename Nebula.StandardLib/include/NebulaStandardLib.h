#pragma once

#ifndef _H_NEBULA_STD_
#define _H_NEBULA_STD_

#ifdef _NEBULA_COMPILING_DLL
#define DLL_API __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif // _NEBULA_COMPILING_DLL

#include "Interpreter.h"
#include "DebuggerDefinitions.h"

namespace nebula::stdlib
{
    nebula::InstructionErrorCode WriteLine(nebula::Interpreter*, nebula::Frame*);
    nebula::InstructionErrorCode Write(nebula::Interpreter*, nebula::Frame*);
    nebula::InstructionErrorCode HashString(nebula::Interpreter*, nebula::Frame*);
}

extern "C"
{
    DLL_API NEB_DECLARE_GET_BINDING(bindingName);
    DLL_API NEB_DECLARE_GET_ALL_BINDINGS;
}

#endif // !_H_NEBULA_STD_

