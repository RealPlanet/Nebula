using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Core.Graph
{
    public sealed class BasicBlockBuilder
    {
        private readonly List<BasicBlock> Blocks = new();
        private readonly List<AbstractStatement> StatementList = new();

        public List<BasicBlock> Build(AbstractBlockStatement block)
        {
            foreach (AbstractStatement? statement in block.Statements)
            {
                switch (statement.Type)
                {
                    case AbstractNodeType.LabelStatement:
                        StartBlock();
                        StatementList.Add(statement);
                        break;
                    case AbstractNodeType.NopStatement:
                    //case BoundNodeType.SEQUENCE_POINT_STATEMENT:
                    case AbstractNodeType.ExpressionStatement:
                    case AbstractNodeType.VariableDeclarationCollection:
                    case AbstractNodeType.WaitStatement:
                    case AbstractNodeType.WaitNotificationStatement:
                    case AbstractNodeType.NotifyStatement:
                        StatementList.Add(statement);
                        break;
                    case AbstractNodeType.ReturnStatement:
                    case AbstractNodeType.ConditionalGotoStatement:
                    case AbstractNodeType.GotoStatement:
                        StatementList.Add(statement);
                        StartBlock();
                        break;
                    default:
                        throw new Exception($"Unexpected statement: {statement.Type}");
                }
            }

            EndBlock();
            return Blocks.ToList();
        }

        private void StartBlock() => EndBlock();

        private void EndBlock()
        {
            if (StatementList.Count > 0)
            {
                BasicBlock block = new();
                block.Statements.AddRange(StatementList);
                Blocks.Add(block);
                StatementList.Clear();
            }
        }
    }
}