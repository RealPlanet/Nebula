using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Declaration
{
    public sealed class VariableDeclaration
        : Node
    {
        public override NodeType Type => NodeType.VariableDeclaration;
        public TypeClause VarType { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public Expression Initializer { get; }

        public VariableDeclaration(SourceCode source, TypeClause type, Token identifier, Token equalsToken, Expression initializer)
            : base(source)
        {
            VarType = type;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return EqualsToken;
            yield return Initializer;
        }
    }
}
