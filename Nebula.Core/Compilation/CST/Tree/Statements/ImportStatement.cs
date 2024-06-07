using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using System.Collections.Generic;

namespace Nebula.Core.Parsing.Statements
{
    public sealed class ImportStatement
        : Statement
    {
        public override NodeType Type => NodeType.ImportStatement;

        public Token ImportKeyword { get; }
        public Token NamespaceString { get; }
        public Token SemicolonToken { get; }

        public string Namespace { get; }

        public override IEnumerable<Node> GetChildren() => throw new System.NotImplementedException();

        public ImportStatement(SourceCode sourceCode, Token importKeyword, Token namespaceString, Token semicolonToken)
           : base(sourceCode)
        {
            ImportKeyword = importKeyword;
            NamespaceString = namespaceString;
            SemicolonToken = semicolonToken;

            string strNamespace = NamespaceString.Text;
            Namespace = strNamespace.Substring(1, strNamespace.Length - 2);
        }
    }
}
