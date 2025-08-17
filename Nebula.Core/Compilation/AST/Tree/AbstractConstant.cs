namespace Nebula.Core.Compilation.AST.Tree
{
    public sealed class AbstractConstant
    {
        public object Value { get; }
        public AbstractConstant(object value)
        {
            Value = value;
        }
    }
}
