using Nebula.Core.Binding;
using Nebula.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Core.Graph
{
    public sealed class GraphBuilder
    {
        private readonly Dictionary<AbstractStatement, BasicBlock> BlockFromStatement = new();
        private readonly Dictionary<AbstractLabel, BasicBlock> BlockFromLabel = new();
        private readonly List<BasicBlockBranch> Branches = new();
        private readonly BasicBlock Start = new(true);
        private readonly BasicBlock End = new(false);
        public ControlFlowGraph Build(List<BasicBlock> blocks)
        {
            if (blocks.Count == 0)
                Connect(Start, End);
            else
                Connect(Start, blocks[0]);

            foreach ((BasicBlock block, AbstractStatement statement) in from block in blocks
                                                                        from statement in block.Statements
                                                                        select (block, statement))
            {
                BlockFromStatement.Add(statement, block);
                if (statement is AbstractLabelStatement l)
                    BlockFromLabel.Add(l.Label, block);
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                BasicBlock? current = blocks[i];
                BasicBlock next = i == blocks.Count - 1 ? End : blocks[i + 1];

                foreach (AbstractStatement? statement in current.Statements)
                {
                    bool isLastStatementInBlock = statement == current.Statements.Last();
                    switch (statement.Type)
                    {
                        case AbstractNodeType.NopStatement:
                        case AbstractNodeType.VariableDeclarationCollection:
                        case AbstractNodeType.ExpressionStatement:
                        //case BoundNodeType.SEQUENCE_POINT_STATEMENT:
                        case AbstractNodeType.WaitStatement:
                        case AbstractNodeType.LabelStatement:
                            {
                                if (isLastStatementInBlock)
                                    Connect(current, next);
                                break;
                            }
                        case AbstractNodeType.ReturnStatement:
                            {
                                Connect(current, End);
                                break;
                            }
                        case AbstractNodeType.ConditionalGotoStatement:
                            {
                                AbstractConditionalGotoStatement cgs = (AbstractConditionalGotoStatement)statement;
                                BasicBlock? thenBlock = BlockFromLabel[cgs.Label];
                                BasicBlock? elseBlock = next;

                                AbstractExpression? negatedCondition = Negate(cgs.Condition);
                                AbstractExpression? thenCondition = cgs.JumpIfTrue ? cgs.Condition : negatedCondition;
                                AbstractExpression? elseCondition = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;

                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                                break;
                            }
                        case AbstractNodeType.GotoStatement:
                            {
                                AbstractGotoStatement gs = (AbstractGotoStatement)statement;
                                BasicBlock? toBlock = BlockFromLabel[gs.Label];
                                Connect(current, toBlock);
                                break;
                            }
                        default:
                            throw new Exception($"Unexpected statement: {statement.Type}");
                    }
                }
            }

        ScanAgain:
            foreach (BasicBlock? block in blocks)
            {
                if (block.Incoming.Count == 0)
                {
                    RemoveBlock(blocks, block);
                    goto ScanAgain;
                }
            }

            blocks.Insert(0, Start);
            blocks.Add(End);

            return new(Start, End, blocks, Branches);
        }

        private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
        {
            blocks.Remove(block);
            foreach (BasicBlockBranch? branch in block.Incoming)
            {
                branch.From.Outgoing.Remove(branch);
                Branches.Remove(branch);
            }

            foreach (BasicBlockBranch? branch in block.Outgoing)
            {
                branch.To.Incoming.Remove(branch);
                Branches.Remove(branch);
            }
        }

        private static AbstractExpression Negate(AbstractExpression condition)
        {
            AbstractUnaryExpression? negated = AbstractNodeFactory.Not(condition.OriginalNode, condition);
            if (negated.ConstantValue != null)
                return new AbstractLiteralExpression(condition.OriginalNode, negated.ConstantValue.Value);

            return negated;
        }

        private void Connect(BasicBlock from, BasicBlock to, AbstractExpression? condition = null)
        {
            if (condition is AbstractLiteralExpression l)
            {
                bool value = (bool)l.Value;
                if (value)
                    condition = null;
                else
                    return;
            }

            BasicBlockBranch branch = new(from, to, condition);
            from.Outgoing.Add(branch);
            to.Incoming.Add(branch);
            Branches.Add(branch);
        }
    }

}