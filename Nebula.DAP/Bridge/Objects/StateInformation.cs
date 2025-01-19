using System.Collections.Generic;

namespace Nebula.Debugger.Bridge.Objects
{
    public class StateInformation
    {
        public Dictionary<int, NebulaThread> Threads { get; } = [];

        public StateInformation(NebulaDebugger parent)
        {
            Parent = parent;
        }

        #region Fields

        internal int FrameIdCounter { get; set; } = 0;
        internal int VariableReferenceCounter { get; set; } = 1;
        public NebulaDebugger Parent { get; }
        #endregion
    }
}
