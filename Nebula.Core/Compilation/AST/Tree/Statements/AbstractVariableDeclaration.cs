using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractVariableDeclaration
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.VariableDeclaration;

        public VariableSymbol Variable { get; }
        public AbstractExpression Initializer { get; }

        public AbstractVariableDeclaration(Node syntax, VariableSymbol variable, AbstractExpression initializer)
            : base(syntax)
        {
            Variable = variable;
            Initializer = initializer;
        }
    }
}
