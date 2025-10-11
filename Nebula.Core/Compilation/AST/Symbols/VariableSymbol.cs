using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public abstract class VariableSymbol
        : Symbol
    {
        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
        public AbstractConstant? Constant { get; }

        public VariableSymbol(string Name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(Name)
        {
            IsReadOnly = isReadOnly;
            Type = variableType;
            Constant = IsReadOnly ? constant : null;
        }
    }
}
