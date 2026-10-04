namespace Nebula.CodeGeneration
{
    public class VariableDefinition
    {
        public int Index { get; }

        /// <summary> The base type of this variable </summary>
        public TypeReference VariableType { get; }

        /// <summary> The name of this variable </summary>
        public string Name { get; }

        public VariableDefinition(TypeReference type,
                                  string name,
                                  int index)
        {
            VariableType = type;
            Name = name;
            Index = index;
        }
    }
}
