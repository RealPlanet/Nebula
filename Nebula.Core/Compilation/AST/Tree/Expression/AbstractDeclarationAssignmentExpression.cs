using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractDeclarationAssignmentExpression
        : AbstractAssignmentExpression
    {
        public override AbstractNodeType Type => AbstractNodeType.DeclarationAssignmentExpression;

        public AbstractDeclarationAssignmentExpression(Node syntax, VariableSymbol variable, AbstractExpression expression)
            : base(syntax, variable, expression)
        {
           
        }
    }
}
