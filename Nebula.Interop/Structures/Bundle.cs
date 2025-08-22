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
            //int fieldCount = 
        }

        private static class NativeMethods
        {

        }
    }
}
