using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{


    public sealed class DebugFile
    {
        [JsonIgnore]
        public string SourceFilePath { get; set; } = string.Empty;

        [JsonInclude]
        public string Namespace { get; init; } = string.Empty;

        [JsonInclude]
        public string OriginalFileName { get; init; } = string.Empty;

        [JsonInclude]
        public string OriginalFileFullName { get; init; } = string.Empty;

        [JsonInclude]
        public string MD5Hash { get; init; } = string.Empty;

        [JsonInclude]
        public Dictionary<string, DebugBundleDefinition> Bundles { get; init; } = [];

        [JsonInclude]
        public Dictionary<string, DebugFunction> Functions { get; init; } = [];

        [JsonInclude]
        public HashSet<string> NativeFunctions { get; init; } = [];

        public static DebugFile LoadFromFile(string filePath)
        {
            string contents = File.ReadAllText(filePath);
            DebugFile? dbgFile = JsonSerializer.Deserialize<DebugFile>(contents);

            if (dbgFile is null)
            {
                throw new InvalidDataException("Could not deserialize debug file");
            }

            return dbgFile;
        }
    }
}
