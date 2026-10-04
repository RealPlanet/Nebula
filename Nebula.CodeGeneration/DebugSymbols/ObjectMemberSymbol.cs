using System.Text.Json.Serialization;

namespace Nebula.CodeGeneration.DebugSymbols
{
    public sealed class ObjectMemberSymbol
    {
        [JsonInclude] public string Name { get; init; } = string.Empty;
        [JsonInclude] public int TypeId { get; init; }
    }
}
