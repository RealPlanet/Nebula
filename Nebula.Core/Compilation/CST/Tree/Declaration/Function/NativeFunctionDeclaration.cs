using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing
{
    public sealed class NativeFunctionDeclaration
        : BaseFunctionDeclaration
    {
        public NativeFunctionDeclaration(SourceCode syntaxTree,
                                         Token keyword,
                                         TypeClause returnType,
                                         Token name,
                                         Token openParenthesis,
                                         TokenSeparatedList<Parameter> parameters,
                                         Token closedParenthesis,
                                         Token semicolon) :
            base(syntaxTree, keyword, returnType, name, openParenthesis, parameters, closedParenthesis)
        {
            Semicolon = semicolon;
        }

        public override NodeType Type => NodeType.FunctionDeclaration;

        public Token Semicolon { get; }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (Node n in base.GetChildren())
                yield return n;

            yield return Semicolon;
        }
    }
}
