using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class BundleField
    {
        public BundleField(TypeIdentifier type, string name)
        {
            Type = type;
            Name = name;
        }

        public BundleField(IntPtr handle)
        {
            Type = (TypeIdentifier)NativeMethods.BundleField_GetType(handle);
            IntPtr str = NativeMethods.BundleField_GetName(handle);
            Name = Marshal.PtrToStringAnsi(str);
        }

        public TypeIdentifier Type { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{Name} {Type}";
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr BundleField_GetName(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int BundleField_GetType(IntPtr handle);
        }
    }
}
