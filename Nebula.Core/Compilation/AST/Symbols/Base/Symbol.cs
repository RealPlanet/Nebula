using Nebula.Core.Utility;
using System.IO;

namespace Nebula.Core.Binding.Symbols
{
    public abstract class Symbol
    {
        public string Name { get; }

        public abstract SymbolType SymbolType { get; }
        private protected Symbol(string name)
        {
            Name = name;
        }

        public void WriteTo(TextWriter writer) => SymbolPrinter.WriteTo(this, writer);
        public override string ToString()
        {
            using (StringWriter writer = new())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}
