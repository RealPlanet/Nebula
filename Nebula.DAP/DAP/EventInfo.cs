using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace Nebula.Debugger.DAP
{
    public struct EventInfo
    {
        public int ThreadId;
        public EventType Type;
    }
}
