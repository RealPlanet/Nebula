using Nebula.Interop.Enumerators;
using System;

namespace Nebula.Interop.Structures
{
    public abstract class Variable
    {
        protected readonly IntPtr _borrowedHandle;

        public TypeIdentifier Type { get; }
        public abstract object Value { get; }

        public Variable(IntPtr instance, TypeIdentifier type)
        {
            _borrowedHandle = instance;
            Type = type;
        }

        public abstract bool Set(int value);

        public abstract bool Set(float value);

        public abstract bool Set(string value);
    }
}
