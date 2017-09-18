using System;
using System.Collections.Generic;

namespace Flayed.Deferment
{
    internal class ConcurrentDictionary<TKey, TValue>
    {
        private readonly object _lock = new object();
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public bool TryAdd(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_dictionary.ContainsKey(key)) return false;
                _dictionary.Add(key, value);
                return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool AddOrUpdate(TKey key, TValue value, Func<TKey, TValue, TValue> updateFunc)
        {
            lock (_lock)
            {
                TValue v;
                if (!TryGetValue(key, out v))
                {
                    return TryAdd(key, value);
                }

                return TryAdd(key, updateFunc(key, v));
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (_lock)
            {
                if (!TryGetValue(key, out value)) return false;
                _dictionary.Remove(key);
                return true;
            }
        }

        public IEnumerable<TValue> Values => _dictionary.Values;
    }
}
