using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nebula.CodeGeneration.DebugSymbols
{
    public sealed class ObjectTypeDebugSymbol
        : BaseTypeDebugSymbol
    {
        [JsonInclude] public string ObjectNamespace { get; init; } = string.Empty;
        [JsonInclude] public string ObjectName { get; init; } = string.Empty;
        [JsonInclude] public List<ObjectMemberSymbol> Members { get; init; } = [];
    }
}
