using Nebula.Interop.Enumerators;
using System;

namespace Nebula.Interop.Structures
{
    public sealed class FunctionAttribute
    {
        public string RawName { get; }

        public AttributeType Type { get; }

        public FunctionAttribute(string name)
        {
            RawName = name;
            if (!Enum.TryParse<AttributeType>(RawName, out var attr))
            {
                Type = AttributeType.Unknown;
            }
        }

        public FunctionAttribute(AttributeType attribute)
        {
            Type = attribute;
            RawName = attribute.ToString();

            if (attribute == AttributeType.Unknown)
            {
                throw new NotSupportedException("Attribute of type unknown is not supported, use from string constructor");
            }
        }
    }
}
