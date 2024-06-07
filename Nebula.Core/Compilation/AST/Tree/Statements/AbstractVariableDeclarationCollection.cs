using Nebula.Commons.Syntax;
using System.Collections.Immutable;

namespace Nebula.Core.Binding
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
    }
}
