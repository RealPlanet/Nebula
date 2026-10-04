using Nebula.Core.Compilation.AST.Symbols.Base;
using Nebula.Core.Compilation.AST.Tree;

namespace Nebula.Core.Compilation.AST.Symbols
{
    public class GlobalVariableSymbol
        : VariableSymbol
    {
        // Namespace of this global variable
        public string Namespace { get; }
        public override SymbolType SymbolType => SymbolType.GlobalVariable;

        public GlobalVariableSymbol(string @namespace, string name, bool isReadOnly, TypeSymbol variableType, AbstractConstant? constant)
            : base(name, isReadOnly, variableType, constant)
        {
            Namespace = @namespace;
        }
    }
}
