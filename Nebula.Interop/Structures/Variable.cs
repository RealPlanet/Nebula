using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Variable
    {
        public TypeIdentifier Type { get; }
        public object Value { get; }

        public Variable(IntPtr instance)
        {
            Type = (TypeIdentifier)NativeMethods.FrameVariable_GetType(instance);
            switch (Type)
            {
                case TypeIdentifier.Int32:
                    {
                        Value = NativeMethods.FrameVariable_GetIntValue(instance);
                        break;
                    }
                case TypeIdentifier.Float:
                    {
                        Value = NativeMethods.FrameVariable_GetFloatValue(instance);
                        break;
                    }
                case TypeIdentifier.String:
                    {
                        IntPtr ptr = NativeMethods.FrameVariable_GetStringValue(instance);
                        Value = Marshal.PtrToStringAnsi(ptr);
                        break;
                    }
                case TypeIdentifier.Bundle:
                    {
                        IntPtr ptr = NativeMethods.FrameVariable_GetBundleValue(instance);
                        Bundle bundle = new Bundle(ptr);
                        Value = bundle;
                        break;
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

        public void Set(string value)
        {
            throw new NotImplementedException();
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
            public static extern IntPtr FrameVariable_GetBundleValue(IntPtr instance);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FrameVariable_GetArrayValue(IntPtr instance);
        }
    }
}
