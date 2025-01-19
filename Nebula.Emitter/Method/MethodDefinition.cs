using Nebula.CodeEmitter.Types;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    public sealed class MethodDefinition
        : ISupportsComments, IEmitterObject
    {
        public HashSet<string> LeadingComments { get; } = [];
        public string Name { get; }
        public NativeAttribute Attributes { get; }
        public TypeReference ReturnType { get; }
        public IList<ParameterDefinition> Parameters { get; } = [];
        public MethodBody Body { get; } = new();
        public TextSpan? SourceCodeTextSpan { get; }

        public MethodDefinition(string name, NativeAttribute attributes, TypeReference returnType, TextSpan span)
        {
            Name = name;
            Attributes = attributes;
            ReturnType = returnType;
            SourceCodeTextSpan = span;
        }
    }
}
