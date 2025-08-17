using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Expressions
{
    /// <summary>
    /// Access a field in a bundle to read/write it's value
    /// </summary>
    public sealed class BundleFieldAccessExpression
        : NameExpression
    {
        public override NodeType Type => NodeType.BundleFieldAccessExpression;
        public Token AccessToken { get; }
        public Token FieldName { get; }

        public BundleFieldAccessExpression(SourceCode sourceCode, Token identifier, Token accessToken, Token fieldName)
            : base(sourceCode, identifier)
        {
            AccessToken = accessToken;
            FieldName = fieldName;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return Identifier;
            yield return AccessToken;
            yield return FieldName;
        }
    }
}
