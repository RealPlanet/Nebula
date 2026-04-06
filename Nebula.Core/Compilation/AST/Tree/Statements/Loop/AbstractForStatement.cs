using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.AST.Tree.Statements.Loop
{
    public sealed class AbstractForStatement
        : AbstractLoopStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.ForStatement;
        public AbstractStatement InitStatement { get; }
        public AbstractExpression? Condition { get; }
        public AbstractExpressionStatement? Expression { get; }
        public AbstractStatement Body { get; }

        public AbstractForStatement(Node syntax, AbstractStatement initStatement, AbstractExpression? condition, AbstractExpressionStatement? expressionStatement, AbstractStatement body,
            AbstractLabel breakLabel, AbstractLabel continueLabel)
            : base(syntax, breakLabel, continueLabel)
        {
            InitStatement = initStatement;
            Condition = condition;
            Expression = expressionStatement;
            Body = body;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            yield return InitStatement;
            if (Condition != null)
            {
                yield return Condition;
            }

            if (Expression != null)
            {
                yield return Expression;
            }

            yield return Body;
        }
    }
}
