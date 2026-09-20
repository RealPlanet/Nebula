using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugVariableSymbols
    {
        /// <summary> The source name of this variable/ field/ parameter </summary>
        [JsonInclude]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Namespace { get; init; }

        /// <summary>
        /// The type contained in the source code (ex: bool, [object_name], int, ecc...)
        /// </summary>
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string TypeName { get; init; } = string.Empty;
    }
}
