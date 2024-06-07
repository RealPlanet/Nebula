using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Core.Parsing
{
    public sealed class VariableDeclarationCollection
        : Statement
    {
        public override NodeType Type => NodeType.VariableDeclarationCollection;
        public Token? ConstKeyword { get; }

        public TokenSeparatedList<VariableDeclaration> Declarations { get; }

        public Token Semicolon { get; }

        public bool IsConst => ConstKeyword != null;

        public VariableDeclarationCollection(SourceCode soureCode, Token? constKeyword, TokenSeparatedList<VariableDeclaration> declarations, Token semicolon)
            : base(soureCode)
        {
            ConstKeyword = constKeyword;
            Declarations = declarations;
            Semicolon = semicolon;
        }

        public VariableDeclarationCollection(SourceCode code, Token? constKeyword, Token semicolon)
            : this(code, constKeyword, new(NodeType.CommaToken), semicolon)
        {
        }

        public override IEnumerable<Node> GetChildren()
        {
            if (ConstKeyword != null)
                yield return ConstKeyword;

            if (Declarations.Count > 0)
            {
                yield return Declarations.First().VarType;

                foreach (VariableDeclaration d in Declarations)
                {
                    yield return d;
                }
            }

            yield return Semicolon;
        }
    }

}
