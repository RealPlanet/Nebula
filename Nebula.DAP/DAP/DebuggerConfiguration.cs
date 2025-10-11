using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System.Collections.Generic;

namespace Nebula.Debugger.DAP
{
    public class DebuggerConfiguration
    {
        public InitializeArguments.PathFormatValue? PathType { get; set; }

        public Dictionary<string, string> Parameters { get; } = [];
        public bool StepOnEntry { get; set; }

        public bool RecompileOnLaunch { get; set; }

        public string CompilerPath { get; set; } = string.Empty;
    }
}
