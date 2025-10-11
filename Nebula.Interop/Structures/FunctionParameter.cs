using Nebula.Interop.Enumerators;

namespace Nebula.Interop.Structures
{
    public sealed class FunctionParameter
    {
        public TypeIdentifier Type { get; }

        public FunctionParameter(TypeIdentifier type)
        {
            Type = type;
        }
    }
}
