using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public class DebugVariable
    {
        [JsonInclude]
        public string Name { get; init; }
    }
}
