using Nebula.Interop.Enumerators;
using Nebula.Interop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Function
    {
        public TypeIdentifier ReturnType { get; }
        public string Name { get; }
        public string Namespace { get; }

        public IReadOnlyList<FunctionParameter> Parameters => _parameters;
        public IReadOnlyList<FunctionAttribute> Attributes => _attributes;
        public IReadOnlyList<Instruction> Instructions => _instructions;

        private readonly List<FunctionParameter> _parameters = new List<FunctionParameter>();
        private readonly List<FunctionAttribute> _attributes = new List<FunctionAttribute>();
        private readonly List<Instruction> _instructions = new List<Instruction>();
        private readonly IntPtr _borrowedHandle;

        public Function(IntPtr handle)
        {
            _borrowedHandle = handle;
            ReturnType = (TypeIdentifier)NativeMethods.Function_GetReturnType(_borrowedHandle);

            IntPtr namePtr = NativeMethods.Function_GetName(_borrowedHandle);
            Name = Marshal.PtrToStringAnsi(namePtr);

            IntPtr namespacePtr = NativeMethods.Function_GetNamespace(_borrowedHandle);
            Namespace = Marshal.PtrToStringAnsi(namePtr);


            LoadAttributes();
            LoadParameters();
            LoadInstructions();
        }

        public override string ToString()
        {
            return Name;
        }

        private void LoadAttributes()
        {
            IntPtr listHandle = NativeMethods.Function_GetAttributes(_borrowedHandle, out int count);
            int[] enumValues = new int[count];
            Marshal.Copy(listHandle, enumValues, 0, count);
            NativeMethods.General_DestroyIntArray(listHandle);

            _attributes.Clear();
            _attributes.Capacity = count;
            foreach (var attr in enumValues.Cast<AttributeType>())
            {
                _attributes.Add(new FunctionAttribute(attr));
            }
        }

        private void LoadParameters()
        {
            IntPtr listHandle = NativeMethods.Function_GetParameters(_borrowedHandle, out int count);
            int[] enumValues = new int[count];
            Marshal.Copy(listHandle, enumValues, 0, count);
            NativeMethods.General_DestroyIntArray(listHandle);

            _parameters.Clear();
            _parameters.Capacity = count;
            foreach (var enumVal in enumValues.Cast<TypeIdentifier>())
            {
                _parameters.Add(new FunctionParameter(enumVal));
            }
        }

        private void LoadInstructions()
        {
            IntPtr listHandle = NativeMethods.Function_GetInstructions(_borrowedHandle, out int count);
            int[] enumValues = new int[count];
            Marshal.Copy(listHandle, enumValues, 0, count);
            NativeMethods.General_DestroyIntArray(listHandle);

            _instructions.Clear();
            _instructions.Capacity = count;
            foreach (var enumVal in enumValues.Cast<InstructionOpcode>())
            {
                _instructions.Add(new Instruction(enumVal));
            }
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Function_GetName(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Function_GetNamespace(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern int Function_GetReturnType(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Function_GetAttributes(IntPtr handle, out int arrLen);
            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Function_GetParameters(IntPtr handle, out int arrLen);

            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Function_GetInstructions(IntPtr handle, out int arrLen);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void General_DestroyIntArray(IntPtr handle);
        }
    }
}
