using System;

namespace Nebula.Debugger.Debugger
{
    public class BreakpointInformation
        : IEquatable<BreakpointInformation?>
    {
        public string Namespace { get; }
        public string FunctionName { get; }
        public int OpcodeIndex { get; }

        public BreakpointInformation(string @namespace, string functionName, int opcodeIndex)
        {
            Namespace = @namespace;
            FunctionName = functionName;
            OpcodeIndex = opcodeIndex;
        }

        public override bool Equals(object? obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (obj is BreakpointInformation bp)
            {
                return this == bp;
            }

            return false;
        }

        public bool Equals(BreakpointInformation? other)
        {
            return this.Equals((object?)other);
        }

        public override string ToString()
        {
            return $"{Namespace}::{FunctionName} | {OpcodeIndex}";
        }

        public override int GetHashCode() => HashCode.Combine(Namespace, FunctionName, OpcodeIndex);

        public static bool operator ==(BreakpointInformation a, BreakpointInformation b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.OpcodeIndex == b.OpcodeIndex && a.Namespace == b.Namespace && a.FunctionName == b.FunctionName;
        }

        public static bool operator !=(BreakpointInformation a, BreakpointInformation b)
        {
            return !(a == b);
        }
    }
}
