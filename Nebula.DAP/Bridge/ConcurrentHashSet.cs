using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Debugger.Bridge
{
    public sealed class ConcurrentHashSet<T>
        : IEnumerable<T>, IReadOnlySet<T>, ICollection<T>
    {
        private readonly object _syncLock = new();
        private readonly HashSet<T> _set = new();

        public int Count => ((IReadOnlyCollection<T>)_set).Count;

        public bool IsReadOnly => false;

        public bool Add(T t)
        {
            lock (_syncLock)
            {
                return _set.Add(t);
            }
        }

        public void Clear()
        {
            lock(_syncLock)
            {
                _set.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock(_syncLock)
            {
                _set.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncLock)
            {
                foreach (var i in _set)
                    yield return i;
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).IsProperSubsetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).IsProperSupersetOf(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).IsSupersetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).Overlaps(other);
            }
        }

        public bool Remove(T item)
        {
            lock(_syncLock)
            {
                return _set.Remove(item);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (_syncLock)
            {
                return ((IReadOnlySet<T>)_set).SetEquals(other);
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncLock)
            {
                foreach (var i in _set)
                    yield return i;
            }
        }
    }
}
