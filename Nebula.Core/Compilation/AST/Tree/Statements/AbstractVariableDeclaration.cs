using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;

namespace Nebula.Core.Binding
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
