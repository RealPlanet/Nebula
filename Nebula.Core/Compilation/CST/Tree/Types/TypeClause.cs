using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Types
{
    public class TypeClause
        : BaseTypeClause
    {
        public override NodeType Type => NodeType.TypeClause;
        public Token Identifier { get; }

        public TypeClause(SourceCode sourceCode, Token identifier)
            : base(sourceCode)
        {
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
        }

        public override string ToString() => Identifier.Text;

        public override bool Equals(object? obj)
        {
            return obj is TypeClause clause &&
                   EqualityComparer<string>.Default.Equals(Identifier.Text, clause.Identifier.Text);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identifier);
        }

        public static bool operator ==(TypeClause? left, TypeClause? right)
        {
            return EqualityComparer<TypeClause>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeClause? left, TypeClause? right)
        {
            return !(left == right);
        }
    }
}
