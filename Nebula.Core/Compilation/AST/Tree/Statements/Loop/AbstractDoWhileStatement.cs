using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements.Loop
{
    public sealed class AbstractDoWhileStatement
        : AbstractLoopStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.DoWhileStatement;
        public AbstractStatement Body { get; }
        public AbstractExpression Condition { get; }

        public AbstractDoWhileStatement(Node syntax, AbstractStatement body, AbstractExpression condition, AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax, breakLabel, continueLabel)
        {
            Body = body;
            Condition = condition;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return Body;
            yield return Condition;
        }
    }
}
