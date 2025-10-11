using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugFunction
    {
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int LineNumber { get; init; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int EndLineNumber { get; init; }

        /// <summary> Number of instructions this function has </summary>
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
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
