using Nebula.CodeEmitter.Types;

namespace Nebula.CodeEmitter
{
    public sealed class ParameterDefinition
    {
        public string Name { get; }
        public TypeReference ParameterType { get; }
        public int Index { get; }

        public ParameterDefinition(TypeReference parameterType, string name, int count)
        {
            ParameterType = parameterType;
            Name = name;
            Index = count;
        }
    }
}
