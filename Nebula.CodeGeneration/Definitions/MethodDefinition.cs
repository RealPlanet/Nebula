using Nebula.CodeGeneration.Interfaces;
using Nebula.Commons.Text;
using Nebula.Interop.Enumerators;
using System.Collections.Generic;

namespace Nebula.CodeGeneration.Definitions
{
    public sealed class MethodDefinition
        : ISupportsComments, IEmitterObject
    {
        public HashSet<string> LeadingComments { get; } = [];
        public string Name { get; }
        public AttributeType Attributes { get; }
        public TypeReference ReturnType { get; }
        public IList<ParameterDefinition> Parameters { get; } = [];
        public MethodBody Body { get; } = new();
        public TextSpan? SourceCodeTextSpan { get; }

        public MethodDefinition(string name, AttributeType attributes, TypeReference returnType, TextSpan span)
        {
            Name = name;
            Attributes = attributes;
            ReturnType = returnType;
            SourceCodeTextSpan = span;
        }
    }
}
