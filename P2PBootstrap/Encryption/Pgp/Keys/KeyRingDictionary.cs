using System.Collections;
using System.Linq;
using System.Text.Json;

namespace P2PBootstrap.Encryption.Pgp.Keys
    {
    /// <summary>
    /// A dictionary that securely stores key pairs in a highly secure manner.
    /// </summary>
    public class KeyRingDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class, IKeyPair
        {

        private readonly Dictionary<TKey, string> _dictionary = new Dictionary<TKey, string>();

        public TValue this[TKey key]
            {
            get
                {
                if (_dictionary.TryGetValue(key, out string value))
                    {
                    return JsonSerializer.Deserialize<TValue>(value);
                    }
                return null;
                }
            set
                {
                _dictionary[key] = JsonSerializer.Serialize(value);
                }
            }

        public ICollection<TKey> Keys => _dictionary.Keys.ToList();

        public ICollection<TValue> Values
            {
            get
                {
                return _dictionary.Values
                    .Select(value => JsonSerializer.Deserialize<TValue>(value)!)
                    .ToList();
                }
            }


        TValue IDictionary<TKey, TValue>.this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => throw new NotImplementedException();

        ICollection<TValue> IDictionary<TKey, TValue>.Values => throw new NotImplementedException();

        int ICollection<KeyValuePair<TKey, TValue>>.Count => throw new NotImplementedException();

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => throw new NotImplementedException();

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
            {
            throw new NotImplementedException();
            }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            {
            throw new NotImplementedException();
            }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
            {
            throw new NotImplementedException();
            }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            {
            throw new NotImplementedException();
            }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
            {
            throw new NotImplementedException();
            }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
            throw new NotImplementedException();
            }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
            throw new NotImplementedException();
            }

        IEnumerator IEnumerable.GetEnumerator()
            {
            throw new NotImplementedException();
            }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
            {
            throw new NotImplementedException();
            }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            {
            throw new NotImplementedException();
            }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
            {
            throw new NotImplementedException();
            }
        }

    }
