namespace Nebula.CodeGeneration
{
    public sealed class GlobalVariableDefinition
        : VariableDefinition
    {
        public string Namespace { get; }

        public GlobalVariableDefinition(TypeReference type, string @namespace, string name, int index)
            : base(type, name, index)
        {
            Namespace = @namespace;
        }
    }
}
