using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class BinaryExpression
        : Expression
    {
        public override NodeType Type => NodeType.BinaryExpression;

        public Expression Left { get; }
        public Token Operator { get; }
        public Expression Right { get; }

        public BinaryExpression(SourceCode sourceCode, Expression left, Token op, Expression right)
            : base(sourceCode)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Left;
            yield return Operator;
            yield return Right;
        }
    }
}
