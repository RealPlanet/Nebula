using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class BundleVariableState
        : Variable
    {
        public override object Value
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BundleVariableState(IntPtr instance)
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
