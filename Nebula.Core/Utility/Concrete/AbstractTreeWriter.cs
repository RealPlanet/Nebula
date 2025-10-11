using Nebula.Commons.Syntax;
using Nebula.Commons.Text.Printers;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;
using Nebula.Core.Compilation.AST.Tree.Expression;
using Nebula.Core.Compilation.AST.Tree.Expression.Bundles;
using Nebula.Core.Compilation.AST.Tree.Statements;
using Nebula.Core.Compilation.AST.Tree.Statements.ControlFlow;
using Nebula.Core.Compilation.AST.Tree.Statements.Loop;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;

namespace Nebula.Core.Utility.Concrete
{
    public static class AbstractTreeWriter
    {
        public static void WriteTo(this AbstractNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
            {
                node.WriteTo(iw);
                return;
            }

            node.WriteTo(new IndentedTextWriter(writer));
        }

        public static void WriteTo(this AbstractNode node, IndentedTextWriter writer)
        {
            switch (node.Type)
            {
                case AbstractNodeType.BlockStatement:
                    WriteBlockStatement((AbstractBlockStatement)node, writer);
                    break;
                case AbstractNodeType.NopStatement:
                    WriteNopStatement((AbstractNopStatement)node, writer);
                    break;
                case AbstractNodeType.WaitStatement:
                    WriteWaitStatement((AbstractWaitStatement)node, writer);
                    break;
                case AbstractNodeType.ExpressionStatement:
                    WriteExpressionStatement((AbstractExpressionStatement)node, writer);
                    break;
                case AbstractNodeType.VariableDeclaration:
                    WriteVariableDeclarationStatement((AbstractVariableDeclaration)node, writer);
                    break;
                case AbstractNodeType.IfStatement:
                    WriteIfStatement((AbstractIfStatement)node, writer);
                    break;
                case AbstractNodeType.WhileStatement:
                    WriteWhileStatement((AbstractWhileStatement)node, writer);
                    break;
                case AbstractNodeType.DoWhileStatement:
                    WriteDoWhileStatement((AbstractDoWhileStatement)node, writer);
                    break;
                case AbstractNodeType.ConditionalGotoStatement:
                    WriteConditionalGotoStatement((AbstractConditionalGotoStatement)node, writer);
                    break;
                case AbstractNodeType.GotoStatement:
                    WriteGotoStatement((AbstractGotoStatement)node, writer);
                    break;
                case AbstractNodeType.LabelStatement:
                    WriteLabelStatement((AbstractLabelStatement)node, writer);
                    break;
                case AbstractNodeType.ReturnStatement:
                    WriteReturnStatement((AbstractReturnStatement)node, writer);
                    break;
                case AbstractNodeType.ErrorExpression:
                    WriteErrorExpression((AbstractErrorExpression)node, writer);
                    break;
                case AbstractNodeType.UnaryExpression:
                    WriteUnaryExpression((AbstractUnaryExpression)node, writer);
                    break;
                case AbstractNodeType.LiteralExpression:
                    WriteLiteralExpression((AbstractLiteralExpression)node, writer);
                    break;
                case AbstractNodeType.BinaryExpression:
                    WriteBinaryExpression((AbstractBinaryExpression)node, writer);
                    break;
                case AbstractNodeType.VariableExpression:
                    WriteVariableExpression((AbstractVariableExpression)node, writer);
                    break;
                case AbstractNodeType.AssignmentExpression:
                    WriteAssignmentExpression((AbstractAssignmentExpression)node, writer);
                    break;
                case AbstractNodeType.CompoundAssignmentExpression:
                    WriteCompoundAssignmentExpression((AbstractCompoundAssignmentExpression)node, writer);
                    break;
                case AbstractNodeType.CallExpression:
                    WriteCallExpression((AbstractCallExpression)node, writer);
                    break;
                case AbstractNodeType.ConversionExpression:
                    WriteConversionExpression((AbstractConversionExpression)node, writer);
                    break;
                case AbstractNodeType.VariableDeclarationCollection:
                    WriteVariableDeclarationCollection((AbstractVariableDeclarationCollection)node, writer);
                    break;
                case AbstractNodeType.ObjectFieldAccessExpression:
                    WriteObjectFieldAccessExpression((AbstractObjectFieldAccessExpression)node, writer);
                    break;
                case AbstractNodeType.ObjectFieldAssignmentExpression:
                    WriteObjectFieldAssignmentExpression((AbstractObjectFieldAssignmentExpression)node, writer);
                    break;
                case AbstractNodeType.InitializationExpression:
                    WriteInitializationExpression((AbstractInitializationExpression)node, writer);
                    break;
                default:
                    throw new Exception($"Unexpected node {node.Type}");
            }
        }

        public static void WriteTo(this AbstractBundleField field, IndentedTextWriter writer)
        {
            writer.WritePunctuation(NodeType.DotToken);
            writer.WriteString(field.FieldName);
        }

        private static void WriteInitializationExpression(AbstractInitializationExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(NodeType.OpenSquareBracketToken);
            writer.Write($"{node.ResultType.BaseType}::{node.ResultType}");
            writer.WritePunctuation(NodeType.ClosedSquareBracketToken);
        }

        private static void WriteObjectFieldAssignmentExpression(AbstractObjectFieldAssignmentExpression node, IndentedTextWriter writer)
        {
            node.Target.WriteTo(writer);
            node.Field.WriteTo(writer);
            writer.WriteSpace();
            writer.WritePunctuation(NodeType.EqualsToken);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteObjectFieldAccessExpression(AbstractObjectFieldAccessExpression node, IndentedTextWriter writer)
        {
            node.Target.WriteTo(writer);
            node.Field.WriteTo(writer);
        }

        private static void WriteVariableDeclarationCollection(AbstractVariableDeclarationCollection node, IndentedTextWriter writer)
        {
            foreach (var n in node.AllVariables)
            {
                WriteTo(n, writer);
            }
        }

        private static void WriteNopStatement(AbstractNopStatement _, IndentedTextWriter writer)
        {
            writer.WriteKeyword("nop");
            writer.WriteLine();
        }

        private static void WriteWaitStatement(AbstractWaitStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(NodeType.WaitKeyword);
            writer.WriteSpace();
            node.TimeExpression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteReturnStatement(AbstractReturnStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(NodeType.ReturnKeyword);
            writer.WriteSpace();
            if (node.Expression is not null)
            {
                node.Expression.WriteTo(writer);
            }
            else
            {
                writer.WriteIdentifier("<void>");
            }

            writer.WriteLine();
        }

        private static void WriteBlockStatement(AbstractBlockStatement node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(NodeType.OpenBracketToken);
            writer.WriteLine();
            writer.Indent++;
            foreach (AbstractStatement? s in node.Statements)
            {
                s.WriteTo(writer);
            }

            writer.Indent--;
            writer.WritePunctuation(NodeType.ClosedBracketToken);
            writer.WriteLine();
        }

        private static void WriteExpressionStatement(AbstractExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteVariableDeclarationStatement(AbstractVariableDeclaration node, IndentedTextWriter writer)
        {
            if (node.Variable.IsReadOnly)
            {
                writer.WriteKeyword(NodeType.ConstKeyword);
                writer.WriteSpace();
            }

            node.Variable.Type.WriteTo(writer);
            writer.WriteSpace();

            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(NodeType.EqualsToken);
            writer.WriteSpace();
            node.Initializer.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteIfStatement(AbstractIfStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(NodeType.IfKeyword);
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.ThenStatement);
            if (node.ElseStatement != null)
            {
                writer.WriteKeyword(NodeType.ElseKeyword);
                writer.WriteLine();
                writer.WriteNestedStatement(node.ElseStatement);
            }
        }

        private static void WriteWhileStatement(AbstractWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(NodeType.WhileKeyword);
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
        }

        private static void WriteDoWhileStatement(AbstractDoWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(NodeType.DoKeyword);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
            writer.WriteLine();
            writer.WriteKeyword(NodeType.WhileKeyword);
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
        }

        private static void WriteGotoStatement(AbstractGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto");
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void WriteConditionalGotoStatement(AbstractConditionalGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto");
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteSpace();
            writer.WriteKeyword(node.JumpIfTrue ? "if" : "unless");
            writer.WriteSpace();
            node.Condition.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteLabelStatement(AbstractLabelStatement node, IndentedTextWriter writer)
        {
            bool unindent = writer.Indent > 0;
            if (unindent)
            {
                writer.Indent--;
            }

            writer.WritePunctuation(node.Label.Name);
            //writer.WritePunctuation(NodeType.ColonToken);
            writer.WriteLine();
            if (unindent)
            {
                writer.Indent++;
            }
        }

        private static void WriteErrorExpression(AbstractErrorExpression _, IndentedTextWriter writer) => writer.WriteKeyword("???");

        private static void WriteLiteralExpression(AbstractLiteralExpression node, IndentedTextWriter writer)
        {
            string value = node.Value.ToString()!;

            if (node.ResultType == TypeSymbol.Bool)
            {
                writer.WriteKeyword((bool)node.Value ? NodeType.TrueKeyword : NodeType.FalseKeyword);
                return;
            }

            if (node.ResultType == TypeSymbol.Int ||
                node.ResultType == TypeSymbol.Float)
            {
                writer.WriteNumber(value);
                return;
            }

            if (node.ResultType == TypeSymbol.String)
            {
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
                writer.WriteString(value);
                return;
            }

            throw new Exception($"Unexpected type {node.Type}");
        }

        private static void WriteCompoundAssignmentExpression(AbstractCompoundAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(node.Operator.NodeType);
            writer.WritePunctuation(NodeType.EqualsToken);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteUnaryExpression(AbstractUnaryExpression node, IndentedTextWriter writer)
        {
            int precedence = node.Operator.NodeType.GetUnaryPrecedence();
            string? textOp = SyntaxEx.GetText(node.Operator.NodeType);
            Debug.Assert(textOp is not null);

            writer.WritePunctuation(textOp);
            writer.WriteNestedExpression(precedence, node.Operand);
        }

        private static void WriteBinaryExpression(AbstractBinaryExpression node, IndentedTextWriter writer)
        {
            int precedence = node.Operator.NodeType.GetBinaryPrecedence();
            string? textOp = SyntaxEx.GetText(node.Operator.NodeType);
            Debug.Assert(textOp is not null);

            writer.WriteNestedExpression(precedence, node.Left);
            writer.WriteSpace();
            writer.WritePunctuation(textOp);
            writer.WriteSpace();
            writer.WriteNestedExpression(precedence, node.Right);
        }

        private static void WriteVariableExpression(AbstractVariableExpression node, IndentedTextWriter writer) => writer.WriteIdentifier(node.Variable.Name);

        private static void WriteAssignmentExpression(AbstractAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(NodeType.EqualsToken);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteCallExpression(AbstractCallExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Function.Name);
            writer.WritePunctuation(NodeType.OpenParenthesisToken);
            bool isFirst = true;
            foreach (AbstractExpression? argument in node.Arguments)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation(NodeType.CommaToken);
                    writer.WriteSpace();
                }
                argument.WriteTo(writer);
            }
            writer.WritePunctuation(NodeType.ClosedParenthesisToken);
        }

        private static void WriteConversionExpression(AbstractConversionExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.ResultType.Name);
            writer.WritePunctuation(NodeType.OpenParenthesisToken);
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(NodeType.ClosedParenthesisToken);
        }

        private static void WriteNestedStatement(this IndentedTextWriter writer, AbstractStatement node)
        {
            bool needsIndent = node is not AbstractBlockStatement;
            if (needsIndent)
            {
                writer.Indent++;
                node.WriteTo(writer);
                writer.Indent--;
                return;
            }

            node.WriteTo(writer);
        }

        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, AbstractExpression node)
        {
            if (node is AbstractUnaryExpression unary)
            {
                writer.WriteNestedExpression(parentPrecedence, unary.Operator.NodeType.GetUnaryPrecedence(), unary);
            }
            else if (node is AbstractBinaryExpression binary)
            {
                writer.WriteNestedExpression(parentPrecedence, binary.Operator.NodeType.GetBinaryPrecedence(), binary);
            }
            else
            {
                node.WriteTo(writer);
            }
        }

        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, AbstractExpression node)
        {
            bool needsParenthesis = parentPrecedence >= currentPrecedence;
            if (needsParenthesis)
            {
                writer.WritePunctuation(NodeType.OpenParenthesisToken);
                node.WriteTo(writer);
                writer.WritePunctuation(NodeType.ClosedParenthesisToken);
                return;
            }

            node.WriteTo(writer);
        }
    }

}
