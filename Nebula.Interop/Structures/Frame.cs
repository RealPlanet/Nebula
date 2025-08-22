using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Frame
    {
        public string Namespace
        {
            get
            {
                IntPtr ptr = NativeMethods.Frame_GetFunctionNamespace(_borrowedHandle);
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public string FunctionName
        {
            get
            {
                IntPtr ptr = NativeMethods.Frame_GetFunctionName(_borrowedHandle);
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public int NextInstructionIndex
        {
            get => NativeMethods.Frame_GetNextInstructionIndex(_borrowedHandle);
        }

        public int InstructionCount
        {
            get => NativeMethods.Frame_GetInstructionCount(_borrowedHandle);
        }

        public int ParameterCount
        {
            get => NativeMethods.Frame_GetParameterCount(_borrowedHandle);
        }

        public int LocalCount
        {
            get => NativeMethods.Frame_GetLocalCount(_borrowedHandle);
        }

        private readonly IntPtr _borrowedHandle;

        public Frame(IntPtr instance)
        {
            _borrowedHandle = instance;
        }

        public Variable GetLocalVariableAt(int i)
        {
            IntPtr ptr = NativeMethods.Frame_GetLocalVariableAt(_borrowedHandle, i);
            return new FrameVariableState(ptr);
        }

        public Variable GetParameterVariableAt(int i)
        {
            IntPtr ptr = NativeMethods.Frame_GetParameterVariableAt(_borrowedHandle, i);
            return new FrameVariableState(ptr);
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Frame_GetFunctionName(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Frame_GetFunctionNamespace(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Frame_GetNextInstructionIndex(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Frame_GetLocalCount(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Frame_GetParameterCount(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Frame_GetLocalVariableAt(IntPtr handle, int index);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Frame_GetParameterVariableAt(IntPtr handle, int index);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Frame_GetInstructionCount(IntPtr handle);
        }
    }
}
