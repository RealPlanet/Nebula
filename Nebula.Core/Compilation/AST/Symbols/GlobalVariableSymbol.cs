using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public class GlobalVariableSymbol
        : VariableSymbol
    {
        public override SymbolType SymbolType => SymbolType.GlobalVariable;

        public GlobalVariableSymbol(string @namespace, string name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(@namespace, name, isReadOnly, variableType, constant)
        {
        }
    }
}
