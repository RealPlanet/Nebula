using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Variable
    {
        private readonly IntPtr _borrowedHandle;

        public TypeIdentifier Type { get; }
        public object Value
        {
            get
            {
                switch (Type)
                {
                    case TypeIdentifier.Int32:
                        {
                            return NativeMethods.FrameVariable_GetIntValue(_borrowedHandle);
                        }
                    case TypeIdentifier.Float:
                        {
                            return NativeMethods.FrameVariable_GetFloatValue(_borrowedHandle);
                        }
                    case TypeIdentifier.String:
                        {
                            IntPtr ptr = NativeMethods.FrameVariable_GetStringValue(_borrowedHandle);
                            return Marshal.PtrToStringAnsi(ptr);
                        }
                    case TypeIdentifier.Bundle:
                        {
                            IntPtr ptr = NativeMethods.FrameVariable_GetBundleValue(_borrowedHandle);
                            Bundle bundle = new Bundle(ptr);
                            return bundle;
                        }
                    case TypeIdentifier.Array:
                    case TypeIdentifier.Void:
                    case TypeIdentifier.Unknown:
                    default:
                        {
                            throw new NotSupportedException($"Variable type '{Type}' not supported");
                        }
                }
            }
        }

        public Variable(IntPtr instance)
        {
            _borrowedHandle = instance;
            Type = (TypeIdentifier)NativeMethods.FrameVariable_GetType(instance);
        }

        public bool Set(int value)
        {
            return NativeMethods.FrameVariable_SetIntValue(_borrowedHandle, value);
        }

        public bool Set(float value)
        {
            return NativeMethods.FrameVariable_SetFloatValue(_borrowedHandle, value);
        }

        public bool Set(string value)
        {
            return NativeMethods.FrameVariable_SetStringValue(_borrowedHandle, value);
        }

        static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int FrameVariable_GetType(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FrameVariable_GetStringValue(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int FrameVariable_GetIntValue(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern float FrameVariable_GetFloatValue(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool FrameVariable_SetStringValue(IntPtr instance, string v);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool FrameVariable_SetIntValue(IntPtr instance, int v);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool FrameVariable_SetFloatValue(IntPtr instance, float v);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FrameVariable_GetBundleValue(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FrameVariable_GetArrayValue(IntPtr instance);
        }
    }
}
