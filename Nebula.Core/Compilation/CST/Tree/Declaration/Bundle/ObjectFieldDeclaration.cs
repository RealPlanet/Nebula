using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Declaration.Bundle
{
    public sealed class ObjectFieldDeclaration
        : Statement
    {
        public BaseTypeClause FieldType { get; }
        public Token Identifier { get; }
        public Token Semicolon { get; }

        public override NodeType Type => NodeType.ObjectFieldDeclaration;

        public ObjectFieldDeclaration(SourceCode code, BaseTypeClause fieldType, Token identifier, Token semicolon)
            : base(code)
        {
            FieldType = fieldType;
            Identifier = identifier;
            Semicolon = semicolon;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (var child in FieldType.GetChildren())
            {
                yield return child;
            }

            yield return Identifier;
            yield return Semicolon;
        }
    }
}
