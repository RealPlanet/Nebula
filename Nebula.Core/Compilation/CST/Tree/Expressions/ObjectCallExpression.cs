using Nebula.Commons.Collections;
using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public class ObjectCallExpression
        : GenericCallExpression
    {
        public override NodeType Type => NodeType.ObjectCallExpression;
        public Expression Target { get; }
        public Token Dot { get; }

        public ObjectCallExpression(SourceCode sourceCode,
                                    Expression target,
                                    Token dot,
                                    Token member,
                                    Token openParenthesis,
                                    TokenSeparatedList<Expression> args,
                                    Token closeParenthesis)
            : base(sourceCode, member, openParenthesis, args, closeParenthesis)
        {
            Target = target;
            Dot = dot;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach(var c in Target.GetChildren())
                yield return c;
            yield return Dot;
            yield return FunctionName;
            yield return OpenParenthesis;
            foreach (Node argument in Arguments.GetWithSeparators())
            {
                yield return argument;
            }

            yield return CloseParenthesis;
        }
    }
}
