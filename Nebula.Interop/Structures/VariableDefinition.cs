using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class VariableDefinition
    {
        public int OrdinalPosition { get; }
        public bool IsConstant { get; }
        public TypeIdentifier Type { get; }
        public string Namespace { get; }
        public string Name { get; }
        public string FullName => $"{Namespace}::{Name}";

        public VariableDefinition(string @namespace, IntPtr handle, int ordinalPosition)
        {
            OrdinalPosition = ordinalPosition;
            Namespace = @namespace;

            IntPtr namePtr = NativeMethods.VariableDefinition_GetName(handle);
            Name = Marshal.PtrToStringAnsi(namePtr) ?? "NO_NAME";

            Type = NativeMethods.VariableDefinition_GetType(handle);
        }

        static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr VariableDefinition_GetName(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern TypeIdentifier VariableDefinition_GetType(IntPtr handle);
        }
    }
}
