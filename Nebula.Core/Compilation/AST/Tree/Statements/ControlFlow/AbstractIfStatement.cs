using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow
{
    public sealed class AbstractIfStatement
        : AbstractStatement
    {
        public AbstractExpression Condition { get; }
        public AbstractStatement ThenStatement { get; }
        public AbstractStatement? ElseStatement { get; }
        public override AbstractNodeType Type => AbstractNodeType.IfStatement;
        public AbstractIfStatement(Node syntax, AbstractExpression condition, AbstractStatement thenStatement, AbstractStatement? elseStatement)
            : base(syntax)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return Condition;
            yield return ThenStatement;
            if (ElseStatement != null)
            {
                yield return ElseStatement;
            }
        }
    }
}
