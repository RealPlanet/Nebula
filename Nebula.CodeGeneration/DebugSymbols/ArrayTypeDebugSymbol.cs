using System.Text.Json.Serialization;

namespace Nebula.CodeGeneration.DebugSymbols
{
    public sealed class ArrayTypeDebugSymbol
        : BaseTypeDebugSymbol
    {
        [JsonInclude]
        public int ArrayTypeId { get; init; }
    }
}
