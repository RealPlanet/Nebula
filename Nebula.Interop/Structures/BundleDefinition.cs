using Nebula.Interop.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{

    public sealed class BundleDefinition
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string FullName => $"{Namespace}::{Name}";

        public IReadOnlyList<BundleField> Fields => _fields;

        private readonly List<BundleField> _fields = new List<BundleField>();

        public BundleDefinition(string @namespace, IntPtr handle)
        {
            Namespace = @namespace;

            IntPtr namePtr = NativeMethods.BundleDefinition_GetName(handle);
            Name = Marshal.PtrToStringAnsi(namePtr) ?? "NO_NAME";


            IntPtr fieldArray = NativeMethods.BundleDefinition_GetFields(handle, out int arrayLen);
            IntPtr[] fields = new IntPtr[arrayLen];
            Marshal.Copy(fieldArray, fields, 0, arrayLen);
            NativeMethods.BundleDefinition_DestroyFieldDefinitionsList(fieldArray);

            for (int i = 0; i < arrayLen; i++)
            {
                _fields.Add(new BundleField(fields[i]));
            }
        }

        static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr BundleDefinition_GetName(IntPtr handle);

            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr BundleDefinition_GetFields(IntPtr handle, out int arrLen);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern void BundleDefinition_DestroyFieldDefinitionsList(IntPtr handle);
        }
    }
}
