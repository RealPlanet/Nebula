using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System.Collections.Generic;
using System.IO;

namespace Nebula.Debugger.DAP
{
    public class DebuggerConfiguration
    {
        public InitializeArguments.PathFormatValue? PathType { get; internal set; }

        public Dictionary<string, string> Parameters { get; } = [];
        public bool StepOnEntry { get; internal set; }
    }
}
