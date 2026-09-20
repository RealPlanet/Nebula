using Nebula.Commons.Syntax;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.CST.Tree.Base;
using Nebula.Core.Compilation.CST.Tree.Expressions;
using Nebula.Core.Compilation.CST.Tree.Types;
using System.Collections.Generic;

namespace Nebula.Core.Compilation.CST.Tree.Declaration
{
    public sealed class VariableDeclaration
        : Statement
    {
        public override NodeType Type => NodeType.VariableDeclaration;

        public BaseTypeClause VarType { get; }

        public AssignmentExpression AssignmentExpression { get; }

        public VariableDeclaration(SourceCode source,
                                   BaseTypeClause type,
                                   AssignmentExpression assignment)
            : base(source)
        {
            VarType = type;
            AssignmentExpression = assignment;
        }

        public override IEnumerable<Node> GetChildren()
        {
            foreach (var child in VarType.GetChildren())
            {
                yield return child;
            }

            yield return AssignmentExpression;
        }
    }
}
