namespace Nebula.CodeGeneration.Definitions
{
    public sealed class ParameterDefinition
    {
        public int Index { get; }

        /// <summary> The base type of this variable </summary>
        public TypeReference VariableType { get; }

        /// <summary> The name of this variable </summary>
        public string Name { get; }

        /// <summary> If this is an object variable, this contains the object definition namespace </summary>
        public string? SourceNamespace { get; set; }

        /// <summary> If this is an object variable, this contains the object definition type name </summary>
        public string? SourceTypeName { get; set; }

        public ParameterDefinition(TypeReference parameterType, string name, int count)
        {
            VariableType = parameterType;
            Name = name;
            Index = count;
        }
    }
}
