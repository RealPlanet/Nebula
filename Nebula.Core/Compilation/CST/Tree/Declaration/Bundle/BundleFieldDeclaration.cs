using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class BundleFieldDeclaration
        : Statement
    {
        public TypeClause FieldType { get; }
        public Token Identifier { get; }
        public Token Semicolon { get; }

        public override NodeType Type => NodeType.BundleFieldDeclaration;

        public BundleFieldDeclaration(SourceCode code, TypeClause fieldType, Token identifier, Token semicolon)
            : base(code)
        {
            FieldType = fieldType;
            Identifier = identifier;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return FieldType;
            yield return Identifier;
            yield return Semicolon;
        }
    }
}
