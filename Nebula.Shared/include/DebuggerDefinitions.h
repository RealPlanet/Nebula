#pragma once

#ifndef _H_NEBULA_DEBUGGER_DEFINITIONS_
#define _H_NEBULA_DEBUGGER_DEFINITIONS_

// I HATE THIS BTW
#define TO_STR2(x) #x
#define TO_STR(x) TO_STR2(x)

#define NEB_GET_ALL_NATIVE_BINDINGS NebulaGetAllNativeFunctions
#define NEB_GET_ALL_NATIVE_BINDINGS_NAME TO_STR(NEB_GET_ALL_NATIVE_BINDINGS)
#define NEB_GET_NATIVE_BINDING NebulaGetNativeFunction
#define NEB_GET_NATIVE_BINDING_NAME TO_STR(NEB_GET_NATIVE_BINDING)

#define NEB_GET_BINDING_PTR nebula::NativeFunctionCallbackPtr(*)(const char*)
#define NEB_GET_ALL_BINDINGS_PTR const std::map<std::string_view, nebula::NativeFunctionCallbackPtr>*(*)(void)
#define NEB_DECLARE_GET_BINDING(bindingName) nebula::NativeFunctionCallbackPtr NEB_GET_NATIVE_BINDING##(const char* bindingName)
#define NEB_DECLARE_GET_ALL_BINDINGS const std::map<std::string_view, nebula::NativeFunctionCallbackPtr>* NEB_GET_ALL_NATIVE_BINDINGS##()

#endif // !_H_NEBULA_DEBUGGER_DEFINITIONS_