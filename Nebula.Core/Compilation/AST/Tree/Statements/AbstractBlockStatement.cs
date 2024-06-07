using Nebula.Commons.Syntax;
using System.Collections.Immutable;

namespace Nebula.Core.Binding
{
    public sealed class AbstractBlockStatement
        : AbstractStatement
    {
        public ImmutableArray<AbstractStatement> Statements { get; }
        public override AbstractNodeType Type => AbstractNodeType.BlockStatement;
        public AbstractBlockStatement(Node syntax, ImmutableArray<AbstractStatement> statements)
            : base(syntax)
        {
            Statements = statements;
        }
    }

}
