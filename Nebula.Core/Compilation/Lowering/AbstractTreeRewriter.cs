using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Expression.Bundles;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow;
using Nebula.Core.Compilation.AST.Tree.Statements.Loop;
using System;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.Lowering
{
    public abstract class AbstractTreeRewriter
    {
        protected virtual AbstractStatement RewriteStatement(AbstractStatement node) => node.Type switch
        {
            AbstractNodeType.BlockStatement => RewriteBlockStatement((AbstractBlockStatement)node),
            AbstractNodeType.NopStatement => RewriteNopStatement((AbstractNopStatement)node),
            AbstractNodeType.ExpressionStatement => RewriteExpressionStatement((AbstractExpressionStatement)node),
            AbstractNodeType.VariableDeclarationCollection => RewriteVariableDeclarationCollectionStatement((AbstractVariableDeclarationCollection)node),
            AbstractNodeType.IfStatement => RewriteIfStatement((AbstractIfStatement)node),
            AbstractNodeType.WhileStatement => RewriteWhileStatement((AbstractWhileStatement)node),
            AbstractNodeType.DoWhileStatement => RewriteDoWhileStatement((AbstractDoWhileStatement)node),
            AbstractNodeType.ForStatement => RewriteForStatement((AbstractForStatement)node),
            AbstractNodeType.LabelStatement => RewriteLabelStatement((AbstractLabelStatement)node),
            AbstractNodeType.GotoStatement => RewriteGotoStatement((AbstractGotoStatement)node),
            AbstractNodeType.ConditionalGotoStatement => RewriteConditionalGotoStatement((AbstractConditionalGotoStatement)node),
            AbstractNodeType.ReturnStatement => RewriteReturnStatement((AbstractReturnStatement)node),
            AbstractNodeType.WaitStatement => RewriteWaitStatement((AbstractWaitStatement)node),
            AbstractNodeType.WaitNotificationStatement => RewriteWaitNotificationStatement((AbstractWaitNotificationStatement)node),
            AbstractNodeType.NotifyStatement => RewriteNotifyStatement((AbstractNotifyStatement)node),
            _ => throw new Exception($"Unexpected node: {node.Type}"),
        };

        protected virtual AbstractStatement RewriteNopStatement(AbstractNopStatement node) => node;

        protected virtual AbstractExpression RewriteExpression(AbstractExpression node) => node.Type switch
        {
            AbstractNodeType.ErrorExpression => RewriteErrorExpression((AbstractErrorExpression)node),
            AbstractNodeType.CompoundAssignmentExpression => RewriteCompoundAssignmentExpression((AbstractCompoundAssignmentExpression)node),
            AbstractNodeType.UnaryExpression => RewriteUnaryExpression((AbstractUnaryExpression)node),
            AbstractNodeType.LiteralExpression => RewriteLiteralExpression((AbstractLiteralExpression)node),
            AbstractNodeType.BinaryExpression => RewriteBinaryExpression((AbstractBinaryExpression)node),
            AbstractNodeType.VariableExpression => RewriteVariableExpression((AbstractVariableExpression)node),
            AbstractNodeType.AssignmentExpression => RewriteAssignmentExpression((AbstractAssignmentExpression)node),
            AbstractNodeType.ObjectFieldAssignmentExpression => RewriteBundleFieldAssignmentExpression((AbstractObjectFieldAssignmentExpression)node),
            AbstractNodeType.InitializationExpression => RewriteInitializationExpression((AbstractInitializationExpression)node),
            AbstractNodeType.ArrayAssignmentExpression => RewriteArrayAssignmentExpression((AbstractArrayAssignmentExpression)node),
            AbstractNodeType.CallExpression => RewriteCallExpression((AbstractCallExpression)node),
            AbstractNodeType.ConversionExpression => RewriteConversionExpression((AbstractConversionExpression)node),
            AbstractNodeType.ObjectCallExpression => RewriteObjectCallExpression((AbstractObjectCallExpression)node),
            AbstractNodeType.ObjectFieldAccessExpression => RewriteObjectFieldAccessExpression((AbstractObjectFieldAccessExpression)node),
            AbstractNodeType.ArrayAccessExpression => RewriteArrayAccessExpression((AbstractArrayAccessExpression)node),
            _ => throw new Exception($"Unexpected node: {node.Type}"),
        };

        protected virtual AbstractExpression RewriteConversionExpression(AbstractConversionExpression node)
        {
            AbstractExpression? expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
            {
                return node;
            }

            return new AbstractConversionExpression(node.OriginalNode, node.ResultType, expression);
        }

        protected virtual AbstractExpression RewriteObjectCallExpression(AbstractObjectCallExpression node)
        {
            ImmutableArray<AbstractExpression>.Builder? builder = null;
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                AbstractExpression? oldArgument = node.Arguments[i];
                AbstractExpression? newArgument = RewriteExpression(oldArgument);

                if (oldArgument != newArgument && builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<AbstractExpression>(node.Arguments.Length);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Add(node.Arguments[j]);
                    }
                }

                builder?.Add(newArgument);
            }

            if (builder == null)
            {
                return node;
            }

            return new AbstractObjectCallExpression(node.OriginalNode, node.Variable, node.Function, builder.MoveToImmutable());

        }

        protected virtual AbstractExpression RewriteObjectFieldAccessExpression(AbstractObjectFieldAccessExpression node)
        {
            var target = RewriteExpression(node.Target);
            if (target == node.Target)
            {
                return node;
            }

            return new AbstractObjectFieldAccessExpression(node.OriginalNode, target, node.Field);
        }

        protected virtual AbstractArrayAccessExpression RewriteArrayAccessExpression(AbstractArrayAccessExpression node)
        {
            var indexExpression = RewriteExpression(node.IndexExpression);
            if (indexExpression == node.IndexExpression)
            {
                return node;
            }

            return new AbstractArrayAccessExpression(node.OriginalNode, node.Variable, indexExpression);
        }

        protected virtual AbstractInitializationExpression RewriteInitializationExpression(AbstractInitializationExpression node)
        {
            return node;
        }

        protected virtual AbstractStatement RewriteConditionalGotoStatement(AbstractConditionalGotoStatement node)
        {
            AbstractExpression? condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
            {
                return node;
            }

            return new AbstractConditionalGotoStatement(node.OriginalNode, node.Label, condition, node.JumpIfTrue);
        }

        protected virtual AbstractStatement RewriteGotoStatement(AbstractGotoStatement node) => node;

        protected virtual AbstractStatement RewriteLabelStatement(AbstractLabelStatement node) => node;

        protected virtual AbstractStatement RewriteWhileStatement(AbstractWhileStatement node)
        {
            AbstractExpression? condition = RewriteExpression(node.Condition);
            AbstractStatement? body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
            {
                return node;
            }

            return new AbstractWhileStatement(node.OriginalNode, condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual AbstractStatement RewriteDoWhileStatement(AbstractDoWhileStatement node)
        {
            AbstractStatement? body = RewriteStatement(node.Body);
            AbstractExpression? condition = RewriteExpression(node.Condition);
            if (condition == node.Condition && body == node.Body)
            {
                return node;
            }

            return new AbstractDoWhileStatement(node.OriginalNode, body, condition, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual AbstractStatement RewriteForStatement(AbstractForStatement node)
        {
            AbstractStatement init = RewriteStatement(node.InitStatement);

            AbstractExpression? condition = null;
            if (node.Condition != null)
            {
                condition = RewriteExpression(node.Condition);
            }

            AbstractExpressionStatement? expression = null;
            if (node.Expression != null)
            {
                expression = RewriteExpressionStatement(node.Expression);
            }

            AbstractStatement? body = RewriteStatement(node.Body);
            if (init == node.InitStatement && condition == node.Condition && body == node.Body)
            {
                return node;
            }

            return new AbstractForStatement(node.OriginalNode, init, condition, expression, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual AbstractStatement RewriteVariableDeclarationCollectionStatement(AbstractVariableDeclarationCollection node)
        {
            ImmutableArray<AbstractVariableDeclaration>.Builder rewrittenDeclarations = ImmutableArray.CreateBuilder<AbstractVariableDeclaration>();

            bool reuseOriginal = true;
            foreach (AbstractVariableDeclaration declaration in node.AllVariables)
            {
                AbstractVariableDeclaration rewrittenDeclaration = RewriteVariableDeclarationStatement(declaration);
                if (rewrittenDeclaration == declaration)
                {
                    rewrittenDeclarations.Add(declaration);
                }
                else
                {
                    reuseOriginal = false;
                    rewrittenDeclarations.Add(rewrittenDeclaration);
                }
            }

            if (reuseOriginal)
            {
                return node;
            }

            return new AbstractVariableDeclarationCollection(node.OriginalNode, rewrittenDeclarations.ToImmutableArray());
        }

        protected virtual AbstractVariableDeclaration RewriteVariableDeclarationStatement(AbstractVariableDeclaration node)
        {
            AbstractExpression? initializer = RewriteExpression(node.Initializer);
            if (initializer == node.Initializer)
            {
                return node;
            }

            return new AbstractVariableDeclaration(node.OriginalNode, node.Variable, initializer);
        }

        protected virtual AbstractStatement RewriteIfStatement(AbstractIfStatement node)
        {
            AbstractExpression? condition = RewriteExpression(node.Condition);
            AbstractStatement? thenStatement = RewriteStatement(node.ThenStatement);
            AbstractStatement? elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);

            if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
            {
                return node;
            }

            return new AbstractIfStatement(node.OriginalNode, condition, thenStatement, elseStatement);
        }

        protected virtual AbstractExpressionStatement RewriteExpressionStatement(AbstractExpressionStatement node)
        {
            AbstractExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
            {
                return node;
            }

            return new AbstractExpressionStatement(node.OriginalNode, expression);
        }

        protected virtual AbstractStatement RewriteBlockStatement(AbstractBlockStatement node)
        {
            ImmutableArray<AbstractStatement>.Builder? builder = null;
            for (int i = 0; i < node.Statements.Length; i++)
            {
                AbstractStatement oldStatement = node.Statements[i];
                AbstractStatement? newStatement = RewriteStatement(oldStatement);
                if (oldStatement != newStatement && builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<AbstractStatement>(node.Statements.Length);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Add(node.Statements[j]);
                    }
                }

                builder?.Add(newStatement);
            }

            if (builder == null)
            {
                return node;
            }

            return new AbstractBlockStatement(node.OriginalNode, builder.MoveToImmutable());
        }

        protected virtual AbstractExpression RewriteErrorExpression(AbstractErrorExpression node) => node;

        protected virtual AbstractExpression RewriteLiteralExpression(AbstractLiteralExpression node) => node;

        protected virtual AbstractExpression RewriteVariableExpression(AbstractVariableExpression node) => node;

        protected virtual AbstractExpression RewriteAssignmentExpression(AbstractAssignmentExpression node)
        {
            AbstractExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
            {
                return node;
            }

            return new AbstractAssignmentExpression(node.OriginalNode, node.Variable, expression);
        }

        protected virtual AbstractExpression RewriteBundleFieldAssignmentExpression(AbstractObjectFieldAssignmentExpression node)
        {
            AbstractExpression target = RewriteExpression(node.Target);
            AbstractExpression expression = RewriteExpression(node.Expression);


            if (target == node.Target && expression == node.Expression)
            {
                return node;
            }

            return new AbstractObjectFieldAssignmentExpression(node.OriginalNode, target, node.Field, expression);
        }

        protected virtual AbstractExpression RewriteArrayAssignmentExpression(AbstractArrayAssignmentExpression node)
        {
            AbstractExpression indexExpression = RewriteExpression(node.IndexExpression);
            AbstractExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression && indexExpression == node.IndexExpression)
            {
                return node;
            }

            return new AbstractArrayAssignmentExpression(node.OriginalNode, node.ArrayVariable, indexExpression, expression);
        }

        protected virtual AbstractExpression RewriteCompoundAssignmentExpression(AbstractCompoundAssignmentExpression node)
        {
            AbstractExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
            {
                return node;
            }

            return new AbstractCompoundAssignmentExpression(node.OriginalNode, node.Variable, node.Operator, expression);
        }

        protected virtual AbstractExpression RewriteUnaryExpression(AbstractUnaryExpression node)
        {
            AbstractExpression operand = RewriteExpression(node.Operand);
            if (operand == node.Operand)
            {
                return node;
            }

            return new AbstractUnaryExpression(node.OriginalNode, node.Operator, operand);
        }

        protected virtual AbstractExpression RewriteBinaryExpression(AbstractBinaryExpression node)
        {
            AbstractExpression? left = RewriteExpression(node.Left);
            AbstractExpression? right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
            {
                return node;
            }

            return new AbstractBinaryExpression(node.OriginalNode, left, node.Operator, right);
        }

        protected virtual AbstractExpression RewriteCallExpression(AbstractCallExpression node)
        {
            ImmutableArray<AbstractExpression>.Builder? builder = null;
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                AbstractExpression? oldArgument = node.Arguments[i];
                AbstractExpression? newArgument = RewriteExpression(oldArgument);

                if (oldArgument != newArgument && builder == null)
                {
                    builder = ImmutableArray.CreateBuilder<AbstractExpression>(node.Arguments.Length);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Add(node.Arguments[j]);
                    }
                }

                builder?.Add(newArgument);
            }

            if (builder == null)
            {
                return node;
            }

            return new AbstractCallExpression(node.OriginalNode, node.IsAsync, node.Namespace, node.Function, builder.MoveToImmutable());
        }

        protected virtual AbstractStatement RewriteReturnStatement(AbstractReturnStatement node)
        {
            AbstractExpression? expression = node.Expression == null ? null : RewriteExpression(node.Expression);
            if (expression == node.Expression)
            {
                return node;
            }

            return new AbstractReturnStatement(node.OriginalNode, expression);
        }

        protected virtual AbstractWaitStatement RewriteWaitStatement(AbstractWaitStatement node)
        {
            AbstractExpression rewrittenExpression = RewriteExpression(node.TimeExpression);
            if (rewrittenExpression == node.TimeExpression)
            {
                return node;
            }

            return new AbstractWaitStatement(node.OriginalNode, rewrittenExpression);
        }

        protected virtual AbstractNotifyStatement RewriteNotifyStatement(AbstractNotifyStatement node)
        {
            AbstractExpression rewrittenExpression = RewriteExpression(node.NotifyExpression);
            if (rewrittenExpression == node.NotifyExpression)
            {
                return node;
            }

            return new AbstractNotifyStatement(node.OriginalNode, node.BundleToNotifyFrom, rewrittenExpression);
        }

        protected virtual AbstractWaitNotificationStatement RewriteWaitNotificationStatement(AbstractWaitNotificationStatement node)
        {
            AbstractExpression rewrittenExpression = RewriteExpression(node.NotifyExpression);
            if (rewrittenExpression == node.NotifyExpression)
            {
                return node;
            }

            return new AbstractWaitNotificationStatement(node.OriginalNode, node.BundleToWaitOn, rewrittenExpression);
        }
    }
}
