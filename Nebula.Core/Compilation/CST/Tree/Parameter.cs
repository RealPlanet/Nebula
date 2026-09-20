using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree
{
    public sealed class Parameter
        : Node
    {
        public override NodeType Type => NodeType.Parameter;

        public BaseTypeClause ParameterType { get; }
        public Token Identifier { get; }

        public Parameter(SourceCode sourceCode, BaseTypeClause type, Token identifier)
            : base(sourceCode)
        {
            ParameterType = type;
            Identifier = identifier;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (var child in ParameterType.GetChildren())
            {
                yield return child;
            }

            yield return Identifier;
        }
    }
}
