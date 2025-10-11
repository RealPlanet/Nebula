using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class BundleVariable
        : Variable
    {
        public override object Value
        {
            get
            {
                switch (Type)
                {
                    case TypeIdentifier.Int32:
                        {
                            return NativeMethods.DataStackVariant_GetIntValue(_borrowedHandle);
                        }
                    case TypeIdentifier.Float:
                        {
                            return NativeMethods.DataStackVariant_GetFloatValue(_borrowedHandle);
                        }
                    case TypeIdentifier.String:
                        {
                            IntPtr ptr = NativeMethods.DataStackVariant_GetStringValue(_borrowedHandle);
                            return Marshal.PtrToStringAnsi(ptr);
                        }
                    case TypeIdentifier.Bundle:
                        {
                            IntPtr ptr = NativeMethods.DataStackVariant_GetBundleValue(_borrowedHandle);
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

        public BundleVariable(IntPtr instance)
            : base(instance, (TypeIdentifier)NativeMethods.DataStackVariant_GetType(instance))
        {

        }


        public override bool Set(int value) => throw new NotImplementedException();
        public override bool Set(float value) => throw new NotImplementedException();
        public override bool Set(string value) => throw new NotImplementedException();

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int DataStackVariant_GetType(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DataStackVariant_GetStringValue(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int DataStackVariant_GetIntValue(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern float DataStackVariant_GetFloatValue(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DataStackVariant_GetBundleValue(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DataStackVariant_GetArrayValue(IntPtr handle);
        }
    }
}
