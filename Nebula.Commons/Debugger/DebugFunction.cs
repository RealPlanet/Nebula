using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    [DataContract]
    public class DebugFunction
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public int LineNumber { get; private set; }

        [DataMember]
        public List<DebugVariable> Parameters { get; private set; } = [];

        [DataMember]
        public List<DebugVariable> LocalVariables { get; private set; } = [];

        /// <summary>
        /// Contains kvps of each new line. <br/>
        /// Key is opcode at which line changes <br/>
        /// Value is line number <br/>
        /// </summary>
        [DataMember]
        public Dictionary<int, int> DeltaInstructionLines { get; private set; } = [];

        public DebugFunction(string name, int lineNumber)
        {
            Name = name;
            LineNumber = lineNumber;
        }

        [JsonConstructor]
        public DebugFunction(string name, int lineNumber, List<DebugVariable> parameters, List<DebugVariable> localVariables, Dictionary<int, int> deltaInstructionLines)
            : this(name, lineNumber)
        {
            Parameters = parameters;
            LocalVariables = localVariables;
            DeltaInstructionLines = deltaInstructionLines;
        }
    }
}
