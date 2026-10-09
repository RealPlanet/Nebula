using System;

namespace Nebula.CodeGeneration.Exceptions
{
    public sealed class EmitterException
        : Exception
    {
        public EmitterException(string? message)
            : base(message)
        {
        }

        public EmitterException()
        {
        }
    }
}
