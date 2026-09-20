using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.CST.Tree.Declaration.Bundle
{
    public sealed class BundleDeclaration
        : Statement
    {
        public TypeClause ObjectType { get; }
        public Token Keyword { get; }
        public Token Name { get; }
        public Token OpenBracket { get; }
        public ImmutableArray<ObjectFieldDeclaration> Fields { get; }
        public Token ClosedBracket { get; }
        public override NodeType Type => NodeType.BundleDeclaration;

        public BundleDeclaration(SourceCode syntaxTree, Token keyword, Token name, Token openBracket, ImmutableArray<ObjectFieldDeclaration> fields, Token closedBracket)
            : base(syntaxTree)
        {
            Keyword = keyword;
            Name = name;
            OpenBracket = openBracket;
            Fields = fields;
            ClosedBracket = closedBracket;
            ObjectType = new TypeClause(syntaxTree, name);
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return Name;
            yield return OpenBracket;
            foreach (ObjectFieldDeclaration v in Fields)
            {
                yield return v;
            }

            yield return ClosedBracket;
        }
    }
}
