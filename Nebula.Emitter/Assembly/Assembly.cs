using Nebula.CodeEmitter.Writer;
using Nebula.Commons.Text;
using System.IO;

namespace Nebula.CodeEmitter
{
    /// <summary>
    /// An assembly to write to file
    /// </summary>
    public sealed class Assembly
    {
        public string ModuleName { get; }
        public Version Version { get; }
        public SourceCode SourceCode { get; }
        public string Namespace { get; }
        public TypeDefinition TypeDefinition { get; } = new();

        public Assembly(string moduleName, string @namespace, Version version, SourceCode sourceCode)
        {
            SourceCode = sourceCode;
            ModuleName = moduleName;
            Namespace = @namespace;
            Version = version;
        }

        public void Write(StreamWriter writer)
        {
            writer.WriteAssembly(this);
        }

        public void WriteDebuggingInfo(StreamWriter writer, string checksum)
        {
            writer.WriterDebuggingInfo(this, checksum);
        }
    }
}
