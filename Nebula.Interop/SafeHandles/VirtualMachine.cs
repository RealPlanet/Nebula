using Microsoft.Win32.SafeHandles;
using Nebula.Interop.Interfaces;
using Nebula.Interop.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nebula.Interop.SafeHandles
{
    public delegate void StdOutEventHandler(string message);
    public delegate void ExitEventHandler();

    public sealed class VirtualMachine
        : SafeHandleZeroOrMinusOneIsInvalid
    {
        public enum State
        {
            Aborted,
            Running,
            Paused,
            Exited,
        }

        public int[] NextOpcodesOfAllThreads
        {
            get
            {
                IntPtr ptr = NativeMethods.Interpreter_GetNextOpcodeForAllThreads(handle, out int arrLen);
                try
                {
                    int[] numbers = new int[arrLen];
                    Marshal.Copy(ptr, numbers, 0, arrLen);
                    return numbers;
                }
                finally
                {
                    NativeMethods.General_DestroyIntArray(ptr);
                }
            }
        }

        public long CurrentThreadId
        {
            get
            {
                return NativeMethods.Interpreter_GetCurrentThreadId(handle);
            }
        }

        public int ThreadCount
        {
            get
            {
                return NativeMethods.Interpreter_GetThreadCount(handle);
            }
        }

        public State VMState
        {
            get
            {
                return (State)NativeMethods.Interpreter_GetState(handle);
            }
        }

        public VirtualMachine()
            : base(true)
        {
            SetHandle(NativeMethods.Interpreter_Create());
        }

        public bool RedirectStdOutput(StdOutEventHandler write, StdOutEventHandler writeLine)
        {
            IntPtr writePtr = Marshal.GetFunctionPointerForDelegate(write);
            IntPtr writeLinePtr = Marshal.GetFunctionPointerForDelegate(writeLine);
            return NativeMethods.Interpreter_RedirectOutput(handle, writePtr, writeLinePtr);
        }

        public bool SetExitCallback(ExitEventHandler callback)
        {
            IntPtr ptr = Marshal.GetFunctionPointerForDelegate(callback);
            return NativeMethods.Interpreter_RedirectExitCallback(handle, ptr);
        }

        public bool ClearRedirectStdOutput()
            => NativeMethods.Interpreter_ClearRedirectOutput(handle);

        public void Initialize(bool startPaused)
        {
            NativeMethods.Interpreter_Init(handle, startPaused);
        }

        public void Run()
        {
            NativeMethods.Interpreter_Run(handle);
        }

        public void Step()
        {
            NativeMethods.Interpreter_Step(handle);
        }

        public void Pause()
        {
            NativeMethods.Interpreter_Pause(handle);
        }

        public void Stop()
        {
            NativeMethods.Interpreter_Stop(handle);
        }

        public void Reset()
        {
            NativeMethods.Interpreter_Reset(handle);
        }

        public bool AddScripts(ICollection<string> scripts, ScriptParseReportCallback onReportMessage)
        {
            string[] arr = scripts.ToArray();
            IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(onReportMessage);
            return NativeMethods.Interpreter_AddScripts(handle, callbackPtr, arr, arr.Length);
        }

        public int GetCurrentOpcodeIndexForThread(int threadId)
        {
            return NativeMethods.Interpreter_GetCurrentOpcodeIndexOfThread(handle, threadId);
        }

        public Callstack GetCallstackOfThread(int threadId)
        {
            IntPtr ptr = NativeMethods.Interpreter_GetCallStackOfThread(handle, threadId);
            return new Callstack(ptr);
        }

        public int AnyFrameJustStarted(string @namespace, string functionName)
        {
            return NativeMethods.Interpreter_AnyFrameJustStarted(handle, @namespace, functionName);
        }

        public int AnyFrameAboutToBeAt(string @namespace, string funcName, int opcodeIndex)
        {
            return NativeMethods.Interpreter_AnyFrameAt(handle, @namespace, funcName, opcodeIndex);
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Interpreter_Destroy(handle);
            SetHandleAsInvalid();
            return true;
        }

        public bool LoadNativesFromDll(string nativeDllBindings, ICollection<string> uniqueNativeFunctions)
        {
            if (!File.Exists(nativeDllBindings))
            {
                return false;
            }

            if (Path.GetExtension(nativeDllBindings).ToLower() != ".dll")
            {
                return false;
            }

            return NativeMethods.Interpreter_LoadSpecificBindingsInDLL(handle,
                                                          nativeDllBindings,
                                                          uniqueNativeFunctions.ToArray(),
                                                          uniqueNativeFunctions.Count);
        }

        public bool LoadNativesFromDll(string nativeDllBindings)
        {
            if (!File.Exists(nativeDllBindings))
            {
                return false;
            }

            if (Path.GetExtension(nativeDllBindings).ToLower() != ".dll")
            {
                return false;
            }

            return NativeMethods.Interpreter_LoadBindingsInDLL(handle, nativeDllBindings);
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Interpreter_Create();
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_RedirectOutput(IntPtr handle, IntPtr writeCb, IntPtr writeLineCb);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_RedirectExitCallback(IntPtr handle, IntPtr callback);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_ClearRedirectOutput(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Init(IntPtr handle, bool startPaused);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Run(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Pause(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Stop(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Reset(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Step(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Interpreter_GetThreadCount(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern long Interpreter_GetCurrentThreadId(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Interpreter_GetState(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_AddScripts(IntPtr handle, IntPtr reportCallback, string[] scriptPaths, int arrLen);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Interpreter_GetNextOpcodeForAllThreads(IntPtr handle, out int arrLen);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_LoadSpecificBindingsInDLL(IntPtr handle, string dllLibrary, string[] functionNames, int arrLen);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool Interpreter_LoadBindingsInDLL(IntPtr handle, string dllLibrary);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Interpreter_AnyFrameJustStarted(IntPtr handle, string @namespace, string funcName);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Interpreter_AnyFrameAt(IntPtr handle, string @namespace, string funcName, int line);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Interpreter_GetCallStackOfThread(IntPtr handle, int threadId);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Interpreter_GetCurrentOpcodeIndexOfThread(IntPtr handle, int threadId);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void General_DestroyIntArray(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Interpreter_Destroy(IntPtr handle);
        }
    }
}
