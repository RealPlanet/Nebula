using Nebula.CodeEmitter.Types;
using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    public sealed class MethodDefinition
        : ISupportsComments
    {
        public HashSet<string> LeadingComments { get; } = [];
        public string Name { get; }
        public NativeAttribute Attributes { get; }
        public TypeReference ReturnType { get; }
        public IList<ParameterDefinition> Parameters { get; } = [];
        public MethodBody Body { get; } = new();

        public MethodDefinition(string name, NativeAttribute attributes, TypeReference returnType)
        {
            Name = name;
            Attributes = attributes;
            ReturnType = returnType;
        }
    }
}
