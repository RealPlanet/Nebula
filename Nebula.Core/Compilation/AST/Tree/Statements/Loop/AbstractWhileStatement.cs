using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Statements.Loop
{
    public sealed class AbstractWhileStatement
        : AbstractLoopStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.WhileStatement;
        public AbstractExpression Condition { get; }
        public AbstractStatement Body { get; }
        public AbstractWhileStatement(Node syntax, AbstractExpression condition, AbstractStatement body,
            AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax, breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
        }
    }
}
