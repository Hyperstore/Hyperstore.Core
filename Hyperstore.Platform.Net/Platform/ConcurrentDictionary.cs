using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform.Net
{
    class ConcurrentDictionaryWrapper<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    {
        public ConcurrentDictionaryWrapper()
            : base()
        {
        }

        bool IConcurrentDictionary<TKey, TValue>.TryAdd(TKey key, TValue value)
        {
            return base.TryAdd(key, value);
        }

        TValue IConcurrentDictionary<TKey, TValue>.GetOrAdd(TKey key, TValue value)
        {
            return base.GetOrAdd(key, value);
        }

        IEnumerable<TValue> IConcurrentDictionary<TKey, TValue>.Values
        {
            get { return base.Values; }
        }

        bool IConcurrentDictionary<TKey, TValue>.TryGetValue(TKey id, out TValue value)
        {
            return base.TryGetValue(id, out value);
        }

        TValue IConcurrentDictionary<TKey, TValue>.this[TKey index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}
