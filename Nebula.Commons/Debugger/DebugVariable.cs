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


        /// <summary>
        /// The type contained in the source code (ex: bool, [object_name], int, ecc...)
        /// </summary>
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string SourceType { get; init; } = string.Empty;

        /// <summary>
        /// The type that will be used internally by the virtual machine (ex: int32, bundle, float, ecc...)
        /// </summary>
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string InternalType { get; init; } = string.Empty;
    }
}
