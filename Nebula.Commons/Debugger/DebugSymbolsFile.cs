using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    public sealed class DebugSymbolsFile
    {
        [JsonIgnore]
        public string SourceFilePath { get; set; } = string.Empty;
        [JsonInclude]
        public string MD5Hash { get; init; } = string.Empty;
        [JsonInclude]
        public string Namespace { get; init; } = string.Empty;

        /// <summary> Types defined in this script </summary>
        [JsonInclude]
        public Dictionary<string, DebugTypeSymbols> Types { get; init; } = [];

        /// <summary> The functions present in this script </summary>
        [JsonInclude]
        public Dictionary<string, DebugFunctionSymbols> Functions { get; init; } = [];

        /// <summary> The native functions referenced by this script </summary>
        [JsonInclude]
        public HashSet<string> NativeFunctions { get; init; } = [];

        public static DebugSymbolsFile LoadFromFile(string filePath)
        {
            string contents = File.ReadAllText(filePath);
            DebugSymbolsFile? dbgFile = JsonSerializer.Deserialize<DebugSymbolsFile>(contents);

            if (dbgFile is null)
            {
                throw new InvalidDataException("Could not deserialize debug file");
            }

            return dbgFile;
        }
    }
}
