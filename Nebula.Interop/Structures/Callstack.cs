using Nebula.Interop.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nebula.Interop.Structures
{
    public sealed class Callstack
    {
        public int FrameCount
        {
            get => NativeMethods.CallStack_GetFrameCount(_borrowedInstance);
        }

        public Frame LastFrame
        {
            get
            {
                int count = FrameCount;
                if (count <= 0)
                {
                    return null;
                }


                IntPtr framePtr = NativeMethods.CallStack_GetFrameAt(_borrowedInstance, FrameCount - 1);
                return new Frame(framePtr);
            }
        }

        public IEnumerable<Frame> Frames
        {
            get
            {
                int count = FrameCount;
                for (int i = 0; i < count; i++)
                {
                    IntPtr framePtr = NativeMethods.CallStack_GetFrameAt(_borrowedInstance, i);
                    yield return new Frame(framePtr);
                }
            }
        }

        private readonly IntPtr _borrowedInstance;

        public Callstack(IntPtr instance)
        {
            _borrowedInstance = instance;
        }

        private static class NativeMethods
        {
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int CallStack_GetFrameCount(IntPtr handle);
            [DllImport(NebulaConstants.DllName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CallStack_GetFrameAt(IntPtr handle, int index);
        }
    }
}
