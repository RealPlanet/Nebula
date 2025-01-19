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
        public TextSpan Span { get; private set; }

        [DataMember]
        public List<DebugVariable> Parameters { get; private set; } = [];

        [DataMember]
        public List<DebugVariable> LocalVariables { get; private set; } = [];

        [DataMember]
        public List<int> InstructionLines { get; private set; } = [];

        public DebugFunction(string name, TextSpan span)
        {
            Name = name;
            Span = span;
        }

        [JsonConstructor]
        public DebugFunction(string name, TextSpan span, List<DebugVariable> parameters, List<DebugVariable> localVariables, List<int> instructionLines)
            : this(name, span)
        {
            Parameters = parameters;
            LocalVariables = localVariables;
            InstructionLines = instructionLines;
        }
    }
}
