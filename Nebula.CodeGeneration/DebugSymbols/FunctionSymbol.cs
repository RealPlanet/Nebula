using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.CodeGeneration.DebugSymbols
{
    public record DebugLine(int LineNumber, int StartOpcodeOfLine);

    public class FunctionSymbol
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
        public List<VariableDebugSymbol> Parameters { get; init; } = [];

        [JsonInclude]
        public List<VariableDebugSymbol> LocalVariables { get; init; } = [];

        /// <summary> The lines of this function with their associated opcode </summary>
        [JsonInclude]
        public List<DebugLine> Lines { get; init; } = [];
    }
}
