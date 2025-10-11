using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugVariable
    {
        [JsonInclude]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? SourceNamespace { get; init; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? SourceType { get; init; }
    }
}
