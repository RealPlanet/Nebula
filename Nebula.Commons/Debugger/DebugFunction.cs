using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Text
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

        [DataMember]
        public List<int> InstructionLines { get; private set; } = [];

        public DebugFunction(string name, int lineNumber)
        {
            Name = name;
            LineNumber = lineNumber;
        }

        [JsonConstructor]
        public DebugFunction(string name, int lineNumber, List<DebugVariable> parameters, List<DebugVariable> localVariables, List<int> instructionLines)
            : this(name, lineNumber)
        {
            Parameters = parameters;
            LocalVariables = localVariables;
            InstructionLines = instructionLines;
        }
    }
}
