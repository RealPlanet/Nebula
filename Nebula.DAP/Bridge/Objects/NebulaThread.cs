using System.Collections.Generic;

namespace Nebula.Debugger.Bridge.Objects
{
    public class NebulaThread
        : BaseNebulaDebuggerObejct
    {
        public int ThreadId { get; }
        public string ThreadName { get; }
        public IList<NebulaStackFrame> StackTrace { get; }

        public NebulaThread(StateInformation parent, int id, IList<NebulaStackFrame> stackTrace)
            : base(parent)
        {
            ThreadId = id;
            ThreadName = $"Thread_{id}";
            StackTrace = stackTrace;
        }
    }
}
