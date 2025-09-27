using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugBundleDefinition
    {
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        public List<DebugVariable> Fields { get; init; } = [];
    }
}
