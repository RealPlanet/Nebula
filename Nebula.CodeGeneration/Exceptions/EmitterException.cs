using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
