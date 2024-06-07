using Nebula.Commons.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Nebula.Core.Parsing
{
    public sealed class TokenSeparatedList<T> : IEnumerable<T>, IReadOnlyCollection<T>
        where T : Node
    {
        #region Properties
        public NodeType Separator { get; }

        public IReadOnlyList<T> Parameters
        {
            get
            {
                List<T> p = new()
                {
                    Capacity = _nodes.Count / 2
                };

                for (int i = 0; i < _nodes.Count; i += 2)
                {
                    p.Add((T)_nodes[i]);
                }

                return p;
            }
        }

        public int Count => Parameters.Count;

        public bool IsReadOnly => true;
        #endregion

        private readonly List<Node> _nodes = new();

        public TokenSeparatedList(NodeType separator)
        {
            Separator = separator;
        }

        public void Append(T p)
        {
            if (_nodes.Count != 0 && _nodes.Last().Type != Separator)
                throw new ArgumentException($"Cannot append {typeof(T).Name} if a separator was not added");

            _nodes.Add(p);
        }

        public void AppendSeparator(Token token)
        {
            if (token.Type != Separator)
                throw new ArgumentException($"Wrong separator type, expected: {Separator}, received: {token.Type}");

            if (_nodes.Count == 0 || _nodes.Last().Type == Separator)
                throw new ArgumentException($"Cannot append separator if a parameter was not added");

            _nodes.Add(token);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _nodes.Count; i += 2)
                yield return (T)_nodes[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        internal Token GetSeparator(int index)
        {
            if (index < 0 || index >= Count - 1)
                throw new ArgumentException(null, nameof(index));

            return (Token)_nodes[index * 2 + 1];
        }

        public ICollection<Node> GetWithSeparators() => _nodes;

        public T this[int index]
        {
            get => (T)_nodes[index * 2];
        }
    }
}
