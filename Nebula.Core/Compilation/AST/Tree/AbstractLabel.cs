namespace Nebula.Core.Binding
{
    public sealed class AbstractLabel
    {
        public string Name { get; }
        internal AbstractLabel(string name)
        {
            Name = name;
        }
        public override string ToString() => Name;
    }
}
