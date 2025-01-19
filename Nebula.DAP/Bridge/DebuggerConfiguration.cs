using System.IO;

namespace Nebula.Debugger.Bridge
{
    public class DebuggerConfiguration
    {
        public DebuggerConfiguration(Stream inStream, Stream outstream)
        {
            InStream = inStream;
            Outstream = outstream;
        }

        public Stream InStream { get; set; }
        public Stream Outstream { get; set; }
        public bool StepOnEntry { get; set; }
    }
}
