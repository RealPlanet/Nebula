using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Operators;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow;
using Nebula.Core.Compilation.AST.Tree.Statements.Loop;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Nebula.Core.Utility.Abstract
{
    internal static class AbstractNodeFactory
    {
        public static AbstractBlockStatement Block(Node syntax, params AbstractStatement[] statements)
        {
            return new AbstractBlockStatement(syntax, ImmutableArray.Create(statements));
        }

        public static AbstractVariableDeclaration VariableDeclaration(Node syntax, VariableSymbol symbol, AbstractExpression initializer)
        {
            return new AbstractVariableDeclaration(syntax, symbol, initializer);
        }

        public static AbstractVariableDeclaration VariableDeclaration(Node syntax, string name, AbstractExpression initializer)
            => VariableDeclarationInternal(syntax, name, initializer, isReadOnly: false);

        public static AbstractVariableDeclaration ConstantDeclaration(Node syntax, string name, AbstractExpression initializer)
            => VariableDeclarationInternal(syntax, name, initializer, isReadOnly: true);

        private static AbstractVariableDeclaration VariableDeclarationInternal(Node syntax, string name, AbstractExpression initializer, bool isReadOnly)
        {
            LocalVariableSymbol local = new(name, isReadOnly, initializer.ResultType, initializer.ConstantValue);
            return new AbstractVariableDeclaration(syntax, local, initializer);
        }

        public static AbstractWhileStatement While(Node syntax, AbstractExpression condition, AbstractStatement body, AbstractLabel breakLabel, AbstractLabel continueLabel)
        {
            return new AbstractWhileStatement(syntax, condition, body, breakLabel, continueLabel);
        }

        public static AbstractGotoStatement Goto(Node syntax, AbstractLabel label)
        {
            return new AbstractGotoStatement(syntax, label);
        }

        public static AbstractConditionalGotoStatement GotoTrue(Node syntax, AbstractLabel label, AbstractExpression condition)
            => new(syntax, label, condition, jumpIfTrue: true);

        public static AbstractConditionalGotoStatement GotoFalse(Node syntax, AbstractLabel label, AbstractExpression condition)
            => new(syntax, label, condition, jumpIfTrue: false);

        public static AbstractLabelStatement Label(Node syntax, AbstractLabel label)
        {
            return new AbstractLabelStatement(syntax, label);
        }

        public static AbstractNopStatement Nop(Node syntax)
        {
            return new AbstractNopStatement(syntax);
        }

        public static AbstractAssignmentExpression Assignment(Node syntax, VariableSymbol variable, AbstractExpression expression)
        {
            return new AbstractAssignmentExpression(syntax, variable, expression);
        }

        public static AbstractBinaryExpression Binary(Node syntax, AbstractExpression left, NodeType kind, AbstractExpression right)
        {
            AbstractBinaryOperator op = AbstractBinaryOperator.Bind(kind, left.ResultType, right.ResultType)!;
            return Binary(syntax, left, op, right);
        }

        public static AbstractBinaryExpression Binary(Node syntax, AbstractExpression left, AbstractBinaryOperator op, AbstractExpression right)
        {
            return new AbstractBinaryExpression(syntax, left, op, right);
        }

        public static AbstractBinaryExpression Add(Node syntax, AbstractExpression left, AbstractExpression right)
            => Binary(syntax, left, NodeType.PlusToken, right);

        public static AbstractBinaryExpression LessOrEqual(Node syntax, AbstractExpression left, AbstractExpression right)
            => Binary(syntax, left, NodeType.LessOrEqualsToken, right);

        public static AbstractExpressionStatement Increment(Node syntax, AbstractVariableExpression variable)
        {
            AbstractBinaryExpression increment = Add(syntax, variable, Literal(syntax, 1));
            AbstractAssignmentExpression incrementAssign = new(syntax, variable.Variable, increment);
            return new AbstractExpressionStatement(syntax, incrementAssign);
        }

        public static AbstractUnaryExpression Not(Node syntax, AbstractExpression condition)
        {
            AbstractUnaryOperator? op = AbstractUnaryOperator.Bind(NodeType.BangToken, TypeSymbol.Bool);
            Debug.Assert(op != null);
            return new AbstractUnaryExpression(syntax, op, condition);
        }

        public static AbstractVariableExpression Variable(Node syntax, AbstractVariableDeclaration variable)
        {
            return Variable(syntax, variable.Variable);
        }

        public static AbstractVariableExpression Variable(Node syntax, VariableSymbol variable)
        {
            return new AbstractVariableExpression(syntax, variable);
        }

        public static AbstractLiteralExpression Literal(Node syntax, object literal)
        {
            Debug.Assert(literal is string || literal is bool || literal is int);

            return new AbstractLiteralExpression(syntax, literal);
        }
    }

}
