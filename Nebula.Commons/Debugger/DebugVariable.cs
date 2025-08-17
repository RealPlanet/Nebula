using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Nebula.Commons.Debugger
{
    [DataContract]
    public class DebugVariable
    {
        [DataMember]
        public string Name { get; private set; }

        [JsonConstructor]
        public DebugVariable(string name)
        {
            Name = name;
        }
    }
}
