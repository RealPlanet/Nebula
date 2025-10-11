using Microsoft.Extensions.Logging;
using Nebula.Debugger.Bridge;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger
{
    public class BreakpointManager
    {
        private readonly ILogger _logger;
        private readonly object _lock = new();

        public IReadOnlyList<BreakpointInformation> FunctionBreakpoints => _functionBreakpoints;
        public IReadOnlyDictionary<string, ConcurrentHashSet<BreakpointInformation>> Breakpoints
            => _breakpoints;

        public readonly List<BreakpointInformation> _functionBreakpoints = [];
        public readonly Dictionary<string, ConcurrentHashSet<BreakpointInformation>> _breakpoints = [];

        public BreakpointManager(ILogger logger)
        {
            _logger = logger;


            _logger.LogInformation($"Breakpoint manager initialized!");
        }

        public void AddFunctionBreakpoint(BreakpointInformation bp)
        {
            // TODO :: Check validity
            lock (_lock)
            {
                _functionBreakpoints.Add(bp);
            }
        }

        public void AddBreakpoint(BreakpointInformation bp)
        {
            // TODO :: Check validity
            lock (_lock)
            {
                if (_breakpoints.TryGetValue(bp.Namespace, out ConcurrentHashSet<BreakpointInformation>? set))
                {
                    set.Add(bp);
                    return;
                }

                _breakpoints[bp.Namespace] =
                [
                    bp
                ];
            }
        }

        public void ClearFunctionBreakpoints()
        {
            // TODO :: Check validity
            lock (_lock)
            {
                _functionBreakpoints.Clear();
            }
        }

        public void ClearBreakpoints()
        {
            lock (_lock)
            {
                _breakpoints.Clear();
            }
        }

        public void ClearBreakpoints(string @namespace)
        {
            lock (_lock)
            {
                if (_breakpoints.TryGetValue(@namespace, out ConcurrentHashSet<BreakpointInformation>? set))
                {
                    set.Clear();
                }
            }

        }
    }
}
