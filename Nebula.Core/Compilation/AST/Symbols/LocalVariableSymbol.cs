using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolType SymbolType => SymbolType.LocalVariable;
        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(name, isReadOnly, variableType, constant)
        {
        }
    }
}
