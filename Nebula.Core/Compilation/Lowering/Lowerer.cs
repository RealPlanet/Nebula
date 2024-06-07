using Nebula.Core.Binding;
using Nebula.Core.Binding.Symbols;
using Nebula.Core.Graph;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Nebula.Core.Utility.AbstractNodeFactory;

namespace Nebula.Core.Lowering
{
    public sealed class Lowerer
        : AbstractTreeRewriter
    {
        private int _labelCount = 0;

        public AbstractBlockStatement Lower(FunctionSymbol function, AbstractStatement body)
        {
            // Rewrite the body of a function to always be a block statement
            AbstractStatement result = RewriteStatement(body);
            return RemoveDeadCode(Flatten(function, result));
        }

        /// <summary>
        /// Break complex statement trees into a flat linear statement chain
        /// </summary>
        private static AbstractBlockStatement Flatten(FunctionSymbol function, AbstractStatement statement)
        {
            ImmutableArray<AbstractStatement>.Builder builder = ImmutableArray.CreateBuilder<AbstractStatement>();
            Stack<AbstractStatement> stack = new();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                AbstractStatement current = stack.Pop();

                if (current is AbstractBlockStatement block)
                {
                    // Reverse them to keep the correct order when pushing into the stack
                    foreach (AbstractStatement? s in block.Statements.Reverse())
                        stack.Push(s);

                    continue;
                }

                builder.Add(current);
            }

            // If a function is empty and doesn't return anything we inject a return statement at the end
            // otherwise we expect a function to always have a return statement
            if (function.ReturnType == TypeSymbol.Void)
            {
                if (builder.Count == 0 || CanFallThrough(builder.Last()))
                {
                    builder.Add(new AbstractReturnStatement(statement.OriginalNode, null));
                }
            }

            return new AbstractBlockStatement(statement.OriginalNode, builder.ToImmutable());
        }

        /// <summary>
        /// True if a statement does not change the flow of the program
        /// </summary>
        private static bool CanFallThrough(AbstractStatement statement)
        {
            return statement.Type != AbstractNodeType.ReturnStatement &&
                   statement.Type != AbstractNodeType.GotoStatement;
        }

        private static AbstractBlockStatement RemoveDeadCode(AbstractBlockStatement node)
        {
            ControlFlowGraph controlFlow = ControlFlowGraph.Create(node);

            //StringWriter s = new StringWriter();
            //controlFlow.WriteTo(s);
            //File.WriteAllText("c.dot", s.ToString());

            // The control flow will only contain reachable
            HashSet<AbstractStatement> reachableStatements = new(controlFlow.Blocks.SelectMany(b => b.Statements));

            // Discard the statement not contained in the graph
            ImmutableArray<AbstractStatement>.Builder builder = node.Statements.ToBuilder();
            for (int i = builder.Count - 1; i >= 0; i--)
            {
                if (!reachableStatements.Contains(builder[i]))
                    builder.RemoveAt(i);
            }

            // Return the new block statement
            return new AbstractBlockStatement(node.OriginalNode, builder.ToImmutable());
        }

        private AbstractLabel GenerateLabel()
        {
            string name = $"Label{++_labelCount}";
            return new AbstractLabel(name);
        }

        #region Overrides

        protected override AbstractStatement RewriteIfStatement(AbstractIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>
                // end:

                AbstractLabel endLabel = GenerateLabel();
                AbstractBlockStatement result = Block(
                    node.OriginalNode,
                    GotoFalse(node.OriginalNode, endLabel, node.Condition),
                    node.ThenStatement,
                    Label(node.OriginalNode, endLabel)
                );

                return RewriteStatement(result);
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:

                AbstractLabel elseLabel = GenerateLabel();
                AbstractLabel endLabel = GenerateLabel();
                AbstractBlockStatement result = Block(
                    node.OriginalNode,
                    GotoFalse(node.OriginalNode, elseLabel, node.Condition),
                    node.ThenStatement,
                    Goto(node.OriginalNode, endLabel),
                    Label(node.OriginalNode, elseLabel),
                    node.ElseStatement,
                    Label(node.OriginalNode, endLabel)
                );

                return RewriteStatement(result);
            }
        }

        protected override AbstractStatement RewriteWhileStatement(AbstractWhileStatement node)
        {
            // while <condition>
            //      <body>
            //
            // ----->
            //
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            AbstractLabel bodyLabel = GenerateLabel();
            AbstractBlockStatement result = Block(
                node.OriginalNode,
                Goto(node.OriginalNode, node.ContinueLabel),
                Label(node.OriginalNode, bodyLabel),
                node.Body,
                Label(node.OriginalNode, node.ContinueLabel),
                GotoTrue(node.OriginalNode, bodyLabel, node.Condition),
                Label(node.OriginalNode, node.BreakLabel)
            );

            return RewriteStatement(result);
        }

        protected override AbstractStatement RewriteDoWhileStatement(AbstractDoWhileStatement node)
        {
            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            AbstractLabel bodyLabel = GenerateLabel();
            AbstractBlockStatement result = Block(
                node.OriginalNode,
                Label(node.OriginalNode, bodyLabel),
                node.Body,
                Label(node.OriginalNode, node.ContinueLabel),
                GotoTrue(node.OriginalNode, bodyLabel, node.Condition),
                Label(node.OriginalNode, node.BreakLabel)
            );

            return RewriteStatement(result);
        }

        protected override AbstractStatement RewriteForStatement(AbstractForStatement node)
        {
            //    // for <init> <condition> <expression>
            //    //      <body>
            //    //
            //    // ---->
            //    //
            //    // {
            //    //      <init>
            //    //      while (condition)
            //    //      {
            //    //          <body>
            //    //          continue:
            //    //            <expression>
            //    //      }
            //    // }

            AbstractStatement initializationStatement = node.InitStatement ?? Nop(node.OriginalNode);
            AbstractExpression conditionExpression = node.Condition ?? Literal(node.OriginalNode, true);
            AbstractStatement postExpression = ((AbstractStatement?)node.Expression) ?? Nop(node.OriginalNode);

            AbstractBlockStatement result = Block(node.OriginalNode,
                                initializationStatement,
                                While(node.OriginalNode,
                                    conditionExpression,
                                        Block(node.OriginalNode,
                                            node.Body,
                                            Label(node.OriginalNode, node.ContinueLabel),
                                            postExpression
                                            ),
                                        node.BreakLabel,
                                        continueLabel: GenerateLabel()
                                        )
                );

            return RewriteStatement(result);
        }

        protected override AbstractStatement RewriteConditionalGotoStatement(AbstractConditionalGotoStatement node)
        {
            if (node.Condition.ConstantValue != null)
            {
                bool condition = (bool)node.Condition.ConstantValue.Value;
                condition = node.JumpIfTrue ? condition : !condition;
                if (condition)
                    return RewriteStatement(Goto(node.OriginalNode, node.Label));
                else
                    return RewriteStatement(Nop(node.OriginalNode));
            }

            return base.RewriteConditionalGotoStatement(node);
        }

        protected override AbstractExpression RewriteCompoundAssignmentExpression(AbstractCompoundAssignmentExpression node)
        {
            AbstractCompoundAssignmentExpression newNode = (AbstractCompoundAssignmentExpression)base.RewriteCompoundAssignmentExpression(node);

            // a <op>= b
            //
            // ---->
            //
            // a = (a <op> b)

            AbstractAssignmentExpression result = Assignment(
                newNode.OriginalNode,
                newNode.Variable,
                Binary(
                    newNode.OriginalNode,
                    Variable(newNode.OriginalNode, newNode.Variable),
                    newNode.Operator,
                    newNode.Expression
                )
            );

            return result;
        }

        #endregion
    }
}
