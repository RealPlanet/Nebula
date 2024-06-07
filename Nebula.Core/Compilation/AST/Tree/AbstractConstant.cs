namespace Nebula.Core.Binding
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
