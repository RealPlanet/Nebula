using Nebula.Core.Binding.Symbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nebula.Core.Binding
{
    public class Scope
    {
        public Scope? Parent { get; }
        public AbstractNamespace Namespace { get; set; } = null!;

        public Scope(Scope? parent)
        {
            Parent = parent;
            // This dictionary could be declared as not nullable but i'd rather keep a lazy allocation instead of an eager one.
            _symbols = null;
        }

        public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);

        public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);

        public bool TryDeclareNativeFunction(FunctionSymbol nativeFunction) => TryDeclareSymbol(nativeFunction);

        public Symbol? TryLookupSymbol(string name)
        {
            if (_symbols != null && _symbols.TryGetValue(name, out Symbol? symbol))
                return symbol;

            return Parent?.TryLookupSymbol(name);
        }
        public IReadOnlyCollection<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();

        public IReadOnlyCollection<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();

        private IReadOnlyCollection<TSymbol> GetDeclaredSymbols<TSymbol>()
            where TSymbol : Symbol
        {
            if (_symbols == null)
                return new List<TSymbol>();

            return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }

        #region Private
        private Dictionary<string, Symbol>? _symbols;

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
            where TSymbol : Symbol
        {
            if (_symbols == null)
                _symbols = new Dictionary<string, Symbol>();
            else if (_symbols.ContainsKey(symbol.Name))
                return false;

            _symbols.Add(symbol.Name, symbol);
            return true;
        }
        #endregion
    }
}
