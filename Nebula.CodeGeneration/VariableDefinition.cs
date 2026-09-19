namespace Nebula.CodeGeneration
{
    public class VariableDefinition
    {
        public int Index { get; }

        /// <summary> The base type of this variable </summary>
        public TypeReference VariableType { get; }

        /// <summary> Namespace of this variable </summary>
        public string Namespace { get; set; }

        /// <summary> The name of this variable </summary>
        public string Name { get; }

        /// <summary> The namespace of the type of this variable (for objects) </summary>
        public string? SourceNamespace { get; set; }

        /// <summary> The original type name (for objects and arrays) </summary>
        public string SourceTypeName { get; set; } = string.Empty;

        public VariableDefinition(TypeReference type,
                                  string namespaceOfType,
                                  string sourceType,
                                  string @namespace,
                                  string name,
                                  int index)
        {
            VariableType = type;
            Namespace = @namespace;
            SourceNamespace = namespaceOfType;
            SourceTypeName = sourceType;
            Name = name;
            Index = index;
        }
    }
}
