using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    public sealed class ObjectFieldInitializationExpression
        : Expression
    {
        public ObjectFieldInitializationExpression(SourceCode sourceCode, Token identifier, Token equals, Expression initializer)
            : base(sourceCode)
        {
            Identifier = identifier;
            EqualsOperator = equals;
            Initializer = initializer;
        }

        public override NodeType Type => NodeType.ObjectFieldInitializationExpression;

        public Token Identifier { get; }
        public Token EqualsOperator { get; }
        public Expression Initializer { get; }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return EqualsOperator;
            yield return Initializer;
        }
    }
}
