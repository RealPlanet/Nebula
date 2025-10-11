using Microsoft.Win32.SafeHandles;
using Nebula.Interop.Interfaces;
using Nebula.Interop.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nebula.Interop.SafeHandles
{

    public delegate void ScriptParseReportCallback(string scriptPath, ReportType type, string message);

    public sealed class Script
        : SafeHandleZeroOrMinusOneIsInvalid
    {
        public string Namespace { get; private set; }

        public IReadOnlyDictionary<string, BundleDefinition> Bundles => _bundles;
        public IReadOnlyDictionary<string, Function> Functions => _functions;

        private readonly Dictionary<string, BundleDefinition> _bundles = new Dictionary<string, BundleDefinition>();
        private readonly Dictionary<string, Function> _functions = new Dictionary<string, Function>();

        private Script(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        public static bool FromFile(string path, ScriptParseReportCallback reportCallback, out Script managedScript)
        {
            IntPtr reportFuncPtr = Marshal.GetFunctionPointerForDelegate(reportCallback);
            IntPtr scriptHandle = NativeMethods.Script_FromFile(path, reportFuncPtr);
            if (scriptHandle == IntPtr.Zero)
            {
                managedScript = null;
                return false;
            }

            managedScript = new Script(scriptHandle);
            managedScript.RefreshFromNative();

            return true;
        }

        public void RefreshFromNative()
        {
            IntPtr namespaceHandle = NativeMethods.Script_GetNamespace(handle);
            Namespace = Marshal.PtrToStringAnsi(namespaceHandle);

            LoadBundlesFromNative();
            LoadFunctionsFromNative();
        }

        private void LoadBundlesFromNative()
        {
            _bundles.Clear();

            IntPtr listHandle = NativeMethods.Script_GetBundleDefinitions(handle, out int count);
            IntPtr[] rawPtrs = new IntPtr[count];
            Marshal.Copy(listHandle, rawPtrs, 0, count);
            NativeMethods.Script_DestroyBundleDefinitionList(listHandle);

            for (int i = 0; i < count; i++)
            {
                IntPtr itemHandle = rawPtrs[i];
                BundleDefinition bundleDef = new BundleDefinition(itemHandle);
                _bundles.Add(bundleDef.Name, bundleDef);
            }
        }

        private void LoadFunctionsFromNative()
        {
            _functions.Clear();

            IntPtr listHandle = NativeMethods.Script_GetFunctions(handle, out int count);
            IntPtr[] rawPtrs = new IntPtr[count];
            Marshal.Copy(listHandle, rawPtrs, 0, count);
            NativeMethods.Script_DestroyFunctionList(listHandle);
            for (int i = 0; i < count; i++)
            {
                IntPtr itemHandle = rawPtrs[i];
                Function func = new Function(itemHandle);
                _functions.Add(func.Name, func);
            }
        }

        protected override bool ReleaseHandle()
        {
            _bundles.Clear();
            _functions.Clear();

            NativeMethods.Script_Destroy(handle);
            SetHandleAsInvalid();

            return true;
        }

        static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Script_FromFile(string path, IntPtr reportingCallback);

            [DllImport(NebulaConstants.DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Script_GetNamespace(IntPtr handle);

            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Script_GetBundleDefinitions(IntPtr handle, out int definitionCount);

            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Script_GetFunctions(IntPtr handle, out int functionCount);

            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Script_Destroy(IntPtr handle);

            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Script_DestroyBundleDefinitionList(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void Script_DestroyFunctionList(IntPtr handle);

        }
    }
}
