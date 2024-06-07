using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;
using System.Collections.Immutable;

namespace Nebula.Core.Binding
{

    public sealed class AbstractCallExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Function.ReturnType;
        public override AbstractNodeType Type => AbstractNodeType.CallExpression;

        public bool IsAsync { get; }
        public string Namespace { get; }

        /// <summary>
        /// The function to call with this call expression.
        /// </summary>
        public FunctionSymbol Function { get; }

        /// <summary>
        /// An Immutable Array of arguments which are passed to the function to be called.
        /// </summary>
        public ImmutableArray<AbstractExpression> Arguments { get; }

        public AbstractCallExpression(Node syntax, bool isAsync, string? @namespace, FunctionSymbol function, ImmutableArray<AbstractExpression> arguments)
            : base(syntax)
        {
            IsAsync = isAsync;
            Namespace = @namespace ?? string.Empty;
            Function = function;
            Arguments = arguments;
        }
    }
}
