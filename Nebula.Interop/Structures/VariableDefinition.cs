using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class VariableDefinition
    {
        public bool IsConstant { get; }
        public TypeIdentifier Type { get; }
        public string Namespace { get; }
        public string Name { get; }
        public string FullName => $"{Namespace}::{Name}";
        public object? ConstantValue { get; } = null;

        public VariableDefinition(IntPtr handle)
        {
            IntPtr namePtr = NativeMethods.VariableDefinition_GetName(handle);
            Name = Marshal.PtrToStringAnsi(namePtr) ?? "NO_NAME";

            IntPtr namespacePtr = NativeMethods.VariableDefinition_GetNamespace(handle);
            Namespace = Marshal.PtrToStringAnsi(namespacePtr) ?? "NO_NAME";
        }

        static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr VariableDefinition_GetName(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr VariableDefinition_GetNamespace(IntPtr handle);
        }
    }
}
