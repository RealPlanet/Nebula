using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Tree.Base;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nebula.Core.Compilation.AST.Tree.Statements
{
    public sealed class AbstractVariableDeclarationCollection
        : AbstractStatement
    {
        public override AbstractNodeType Type => AbstractNodeType.VariableDeclarationCollection;

        public ImmutableArray<AbstractVariableDeclaration> AllVariables { get; }

        public AbstractVariableDeclarationCollection(Node syntax, ImmutableArray<AbstractVariableDeclaration> allVariables)
            : base(syntax)
        {
            AllVariables = allVariables;
        }

        public override IEnumerable<AbstractNode> GetChildren()
        {
            foreach (var variable in AllVariables)
                yield return variable;
        }
    }
}
