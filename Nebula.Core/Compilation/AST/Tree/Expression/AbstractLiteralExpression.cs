using Nebula.Commons.Syntax;
using Nebula.Core.Binding.Symbols;
using System;

namespace Nebula.Core.Binding
{
    public sealed class AbstractLiteralExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType { get; }
        public override AbstractNodeType Type => AbstractNodeType.LiteralExpression;
        public object Value => ConstantValue.Value;
        public override AbstractConstant ConstantValue { get; }

        public AbstractLiteralExpression(Node syntax, object value)
            : base(syntax)
        {
            ResultType = value switch
            {
                bool => TypeSymbol.Bool,
                int => TypeSymbol.Int,
                string => TypeSymbol.String,
                _ => throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'"),
            };

            ConstantValue = new(value);
        }
    }
}
