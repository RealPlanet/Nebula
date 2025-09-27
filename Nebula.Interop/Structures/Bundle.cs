using Nebula.Interop.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Bundle
    {
        public IReadOnlyList<Variable> Fields => _fields;

        private readonly List<Variable> _fields = new List<Variable>();

        public Bundle(IntPtr instance)
        {
            int fieldCount = NativeMethods.Bundle_GetFieldCount(instance);
            for (int i = 0; i < fieldCount; i++)
            {
                IntPtr bundleVariable = NativeMethods.Bundle_GetField(instance, i);
                BundleVariable variable = new BundleVariable(bundleVariable);
                _fields.Add(variable);
            }
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Bundle_GetFieldCount(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Bundle_GetField(IntPtr handle, int index);
        }
    }
}
