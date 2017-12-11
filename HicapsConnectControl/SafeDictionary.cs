using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectControl
{
    public class SafeDictionary<TKey, TValue>
    {
        private readonly object _Padlock = new object();
        private readonly Dictionary<TKey, TValue> _Dictionary = new Dictionary<TKey, TValue>();


        public TValue this[TKey key]
        {
            get
            {
                lock (_Padlock)
                {
                    return _Dictionary[key];
                }
            }

            set
            {
                lock (_Padlock)
                {
                    _Dictionary[key] = value;
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_Padlock)
            {
                return _Dictionary.TryGetValue(key, out value);
            }
        }
    }

}
