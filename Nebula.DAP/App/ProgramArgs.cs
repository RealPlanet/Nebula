using Ookii.CommandLine;
using System.ComponentModel;

namespace Nebula.Debugger
{
    [GeneratedParser]
    internal partial class ProgramArgs
    {
        [CommandLineArgument("server_port", DefaultValue = 0, IsRequired = false)]
        [Description("Server port")]
        public int ServerPort { get; set; }
    }
}
