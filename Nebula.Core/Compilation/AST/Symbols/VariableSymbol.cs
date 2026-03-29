using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public abstract class VariableSymbol
        : Symbol
    {
        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
        public AbstractConstant? Constant { get; private set; }

        public VariableSymbol(string @namespace, string name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(@namespace, name)
        {
            IsReadOnly = isReadOnly;
            Type = variableType;
            SetConstant(constant);
        }

        public void SetConstant(AbstractConstant? constant)
        {
            Constant = IsReadOnly ? constant : null;
        }
    }
}
