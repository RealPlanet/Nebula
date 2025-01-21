using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Debugger.Bridge
{

    public class BreakpointManager
    {
        public NebulaDebugger Debugger { get; }

        public IReadOnlySet<NebulaBreakpoint> FunctionBreakpoints => _functionBreakpoints;

        public IReadOnlyList<ConcurrentHashSet<NebulaBreakpoint>> GenericBreakpoints
        {
            get
            {
                lock(_syncLock)
                {
                    return _breakpoints.Values.ToList();
                }
            }
        }

        /// <summary>
        /// Line berakpoints, key is namespace
        /// </summary>
        private readonly ConcurrentHashSet<NebulaBreakpoint> _functionBreakpoints = [];
        private readonly ConcurrentDictionary<string, ConcurrentHashSet<NebulaBreakpoint>> _breakpoints = [];
        private readonly object _syncLock = new();

        public BreakpointManager(NebulaDebugger owner)
        {
            Debugger = owner;
        }

        internal void AddFunctionBreakpoint(NebulaBreakpoint bp)
        {
            _functionBreakpoints.Add(bp);
        }

        public void AddBreakpoint(NebulaBreakpoint bp)
        {
            if (_breakpoints.TryGetValue(bp.Namespace, out var set))
            {
                set.Add(bp);
                return;
            }

            _breakpoints[bp.Namespace] =
            [
                bp
            ];
        }

        internal void ClearGenericBreakpoints(string @namespace)
        {
            _breakpoints[@namespace] = [];
        }

        internal void ClearFunctionBreakpoints() => _functionBreakpoints.Clear();
    }
}
