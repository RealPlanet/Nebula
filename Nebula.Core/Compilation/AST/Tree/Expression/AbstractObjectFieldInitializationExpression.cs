using Nebula.Commons.Syntax;
using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree.Base;

namespace Nebula.Core.Compilation.AST.Tree.Expression
{
    public sealed class AbstractObjectFieldInitializationExpression
        : AbstractExpression
    {
        public override TypeSymbol ResultType => Initializer.ResultType;

        public override AbstractNodeType Type => AbstractNodeType.ObjectFieldInitializationExpression;

        public string FieldName { get; }

        public AbstractBundleField? Field { get; private set; }

        public AbstractExpression Initializer { get; private set; }

        public AbstractObjectFieldInitializationExpression(Node originalNode, string fieldName, AbstractExpression initializer)
            : base(originalNode)
        {
            FieldName = fieldName;
            Initializer = initializer;
        }

        public AbstractObjectFieldInitializationExpression(Node originalNode, string fieldName, AbstractBundleField field, AbstractExpression initializer)
            : this(originalNode, fieldName, initializer)
        {
            SetFieldToInitialize(field);
        }

        public void SetFieldToInitialize(AbstractBundleField field)
        {
            Field = field;
            if(Initializer is AbstractObjectInitializationExpression e)
            {
                e.SetAllocationResult(Field.FieldType);
            }
        }

        public void SetFieldInitializer(AbstractExpression expr)
        {
            Initializer = expr;
        }
    }
}
