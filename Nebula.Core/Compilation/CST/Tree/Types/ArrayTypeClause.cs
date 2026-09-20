using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Types
{

    public class ArrayTypeClause
        : BaseTypeClause
    {
        public override NodeType Type => NodeType.ArrayTypeClause;
        public BaseTypeClause TypeOfArray { get; }
        public Token OpenBracket { get; }
        public Token ClosedBracket { get; }

        public ArrayTypeClause(SourceCode sourceCode, BaseTypeClause type, Token open, Token close) 
            : base(sourceCode)
        {
            TypeOfArray = type;
            OpenBracket = open;
            ClosedBracket = close;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach(var node in TypeOfArray.GetChildren())
            {
                yield return node;
            }

            yield return TypeOfArray;
            yield return OpenBracket;
            yield return ClosedBracket;
        }
    }
}
