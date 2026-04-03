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

        public TypeClause VarType { get; }

        public AssignmentExpression AssignmentExpression { get; }

        public VariableDeclaration(SourceCode source,
                                   TypeClause type,
                                   AssignmentExpression assignment)
            : base(source)
        {
            VarType = type;
            AssignmentExpression = assignment;
        }

        public override IEnumerable<Node> GetChildren()
        {
            yield return VarType;
            yield return AssignmentExpression;
        }
    }
}
