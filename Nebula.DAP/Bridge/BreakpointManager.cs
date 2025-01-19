using System.Collections.Generic;

namespace Nebula.Debugger.Bridge
{
    public class BreakpointManager
    {
        public NebulaDebugger Debugger { get; }
        private readonly HashSet<int> _breakpoints = [];

        public BreakpointManager(NebulaDebugger owner)
        {
            Debugger = owner;
        }

        public bool HasLineBreakpoint(int line)
        {
            return _breakpoints.Contains(line);
        }

        public void AddBreakpoint(int line)
        {
            _breakpoints.Add(line);
        }
    }
}
