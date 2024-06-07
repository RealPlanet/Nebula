using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Core.Parsing
{
    public sealed class BundleDeclaration
        : Statement
    {
        public Token Keyword { get; }
        public Token Name { get; }
        public Token OpenBracket { get; }
        public ImmutableArray<BundleFieldDeclaration> Fields { get; }
        public Token ClosedBracket { get; }
        public override NodeType Type => NodeType.BundleDeclaration;

        public BundleDeclaration(SourceCode syntaxTree, Token keyword, Token name, Token openBracket, ImmutableArray<BundleFieldDeclaration> fields, Token closedBracket)
            : base(syntaxTree)
        {
            Keyword = keyword;
            Name = name;
            OpenBracket = openBracket;
            Fields = fields;
            ClosedBracket = closedBracket;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Keyword;
            yield return Name;
            yield return OpenBracket;
            foreach (BundleFieldDeclaration v in Fields)
                yield return v;
            yield return ClosedBracket;
        }
    }
}
