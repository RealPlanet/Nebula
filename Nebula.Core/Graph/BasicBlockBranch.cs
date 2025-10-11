using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Graph
{
    public sealed class BasicBlockBranch
    {
        public BasicBlock From { get; }
        public BasicBlock To { get; }
        public AbstractExpression? Condition { get; }
        public BasicBlockBranch(BasicBlock from, BasicBlock to, AbstractExpression? condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
        public override string ToString()
        {
            if (Condition is null)
            {
                return string.Empty;
            }

            return Condition.ToString()!;
        }
    }
}