using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    /// <summary>
    /// Access a field in a bundle to read/write it's value
    /// </summary>
    public sealed class ObjectFieldAccess
        : Expression
    {
        public override NodeType Type => NodeType.ObjectFieldAccessExpression;

        public Expression Target { get; }
        public Token Dot { get; }
        public Token Member { get; }

        public ObjectFieldAccess(SourceCode source, Expression target, Token dot, Token member)
            : base(source)
        {
            Target = target;
            Dot = dot;
            Member = member;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach(var c in Target.GetChildren())
                yield return c;

            yield return Dot;
            yield return Member;
        }
    }
}
