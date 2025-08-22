using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugFunction
    {
        [JsonInclude]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        public int LineNumber { get; init; }

        [JsonInclude]
        public int EndLineNumber { get; init; }

        /// <summary> Number of instructions this function has </summary>
        [JsonInclude]
        public long InstructionCount { get; init; }

        [JsonInclude]
        public List<DebugVariable> Parameters { get; init; } = [];

        [JsonInclude]
        public List<DebugVariable> LocalVariables { get; init; } = [];

        [JsonInclude]
        public Dictionary<int, int> LineStartingOpcodeIndex { get; init; } = [];

        [JsonInclude]
        public List<int> Statements { get; init; } = [];
    }
}
