using System.Collections.Generic;

namespace Nebula.CodeEmitter
{
    public sealed class MethodBody
    {
        public IList<VariableDefinition> Arguments { get; } = [];
        public IList<Instruction> Instructions { get; } = [];
        public IList<VariableDefinition> Variables { get; } = [];

        public NILProcessor NILProcessor => new(this);
    }
}
