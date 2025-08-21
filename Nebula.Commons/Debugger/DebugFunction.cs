using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugFunction
    {
        [JsonInclude]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        public int LineNumber { get; init; }

        /// <summary> Number of instructions this function has </summary>
        [JsonInclude]
        public long InstructionCount { get; init; }

        [JsonInclude]
        public List<DebugVariable> Parameters { get; init; } = [];

        [JsonInclude]
        public List<DebugVariable> LocalVariables { get; init; } = [];

        /// <summary>
        /// Contains kvps of each new line. <br/>
        /// Key is opcode at which line changes <br/>
        /// Value is line number <br/>
        /// </summary>
        [JsonInclude]
        public Dictionary<int, int> DeltaInstructionLines { get; init; } = [];

        [JsonInclude]
        public List<int> NewStatementIndex { get; init; } = [];
    }
}
