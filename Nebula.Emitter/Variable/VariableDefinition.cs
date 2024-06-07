
using Nebula.CodeEmitter.Types;

namespace Nebula.CodeEmitter
{
    public class VariableDefinition
    {
        public int Index { get; }
        public TypeReference VariableType { get; }
        public string Name { get; }

        public VariableDefinition(TypeReference type, string name, int index)
        {
            VariableType = type;
            Name = name;
            Index = index;
        }
    }
}
