using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    [DataContract]
    public sealed class DebugFile
    {
        [DataMember]
        public string Namespace { get; private set; }
        [DataMember]
        public string OriginalFileName { get; private set; }

        [DataMember]
        public string MD5Hash { get; private set; }

        [DataMember]
        public Dictionary<string, DebugFunction> Functions { get; private set; } = [];

        [DataMember]
        public HashSet<string> NativeFunctions { get; private set; } = [];


        public DebugFile(string @namespace, string originalFileName, string md5Hash)
        {
            Namespace = @namespace;
            MD5Hash = md5Hash;
            OriginalFileName = originalFileName;
        }

        [JsonConstructor]
        public DebugFile(string @namespace, string originalFileName, string mD5Hash, Dictionary<string, DebugFunction> functions, HashSet<string> nativeFunctions)
            : this(@namespace, originalFileName, mD5Hash)
        {
            Functions = functions;
            NativeFunctions = nativeFunctions;
        }

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
