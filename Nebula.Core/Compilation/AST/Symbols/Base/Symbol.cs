using Nebula.Core.Utility;
using System.IO;

namespace Nebula.Core.Compilation.AST.Symbols.Base
{
    public abstract class Symbol
    {
        public string Namespace { get; }
        public string Name { get; }

        public abstract SymbolType SymbolType { get; }

        private protected Symbol(string @namespace, string name)
        {
            Namespace = @namespace;
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
