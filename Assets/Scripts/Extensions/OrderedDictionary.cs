using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;

// see https://stackoverflow.com/a/35992747

// ReSharper disable CheckNamespace -- be available where Dictionary<,> is

namespace System.Collections.Generic
{
    /// <summary>
    ///     System.Collections.Specialized.OrderedDictionary is NOT generic.
    ///     This class is essentially a generic wrapper for OrderedDictionary.
    ///     https://github.com/Microsoft/referencesource/blob/master/System.ServiceModel.Internals/System/Runtime/Collections/OrderedDictionary.cs
    /// </summary>
    /// <remarks>
    ///     Indexer here will NOT throw KeyNotFoundException
    /// </remarks>
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private readonly OrderedDictionary _privateDictionary;

        /// <summary>
        /// Initializes a new mutable instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        public OrderedDictionary()
        {
            _privateDictionary = new OrderedDictionary();
        }

        /// <summary>
        /// Initializes a new READ ONLY instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        private OrderedDictionary([NotNull] IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            var orderedDictionary = new OrderedDictionary(dictionary.Count);

            foreach (var pair in dictionary)
            {
                orderedDictionary.Add(pair.Key, pair.Value);
            }

            _privateDictionary = orderedDictionary.AsReadOnly();
        }

        int ICollection.Count => _privateDictionary.Count;
        object ICollection.SyncRoot => ((ICollection)_privateDictionary).SyncRoot;
        bool ICollection.IsSynchronized => ((ICollection)_privateDictionary).IsSynchronized;
        void ICollection.CopyTo(Array array, int index) => _privateDictionary.CopyTo(array, index);

        ICollection IDictionary.Keys => _privateDictionary.Keys;
        ICollection IDictionary.Values => _privateDictionary.Values;
        bool IDictionary.IsFixedSize => ((IDictionary)_privateDictionary).IsFixedSize;
        bool IDictionary.IsReadOnly => _privateDictionary.IsReadOnly;
        void IDictionary.Add(object key, object value) => _privateDictionary.Add(key, value);
        void IDictionary.Clear() => _privateDictionary.Clear();
        bool IDictionary.Contains(object key) => _privateDictionary.Contains(key);
        void IDictionary.Remove(object key) => _privateDictionary.Remove(key);
        IDictionaryEnumerator IDictionary.GetEnumerator() => _privateDictionary.GetEnumerator();

        object IDictionary.this[object key]
        {
            get { return _privateDictionary[key]; }
            set { _privateDictionary[key] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var enumerable = from DictionaryEntry entry in _privateDictionary
                             select new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);

            return enumerable.GetEnumerator();
        }

        public bool IsReadOnly => _privateDictionary.IsReadOnly;
        public int Count => _privateDictionary.Count;

        /// <summary>
        ///     Gets or sets the <see cref="TValue" /> with the specified key.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                if (_privateDictionary.Contains(key))
                {
                    return (TValue)_privateDictionary[key];
                }

                return default(TValue);
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                _privateDictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>(_privateDictionary.Count);

                keys.AddRange(_privateDictionary.Keys.Cast<TKey>());

                return keys.AsReadOnly();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(_privateDictionary.Count);

                values.AddRange(_privateDictionary.Values.Cast<TValue>());

                return values.AsReadOnly();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        ///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            _privateDictionary.Add(key, value);
        }

        public void Clear() => _privateDictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (item.Key == null || !_privateDictionary.Contains(item.Key)) return false;

            return _privateDictionary[item.Key].Equals(item.Value);
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the
        ///     specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        ///     true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise,
        ///     false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return _privateDictionary.Contains(key);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="System.ArgumentNullException">array</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex is &lt; 0</exception>
        /// <exception cref="System.ArgumentException">
        ///     Cannot copy to array, array dimensions insufficient
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex = 0)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Rank > 1) throw new ArgumentException($"array.Rank of {array.Rank} exceeds dimensions allowed (1)");
            if (arrayIndex >= array.Length || array.Length - arrayIndex < _privateDictionary.Count)
                throw new ArgumentException("Cannot copy to array, array dimensions insufficient", nameof(array));

            var index = arrayIndex;
            foreach (DictionaryEntry entry in _privateDictionary)
            {
                array[index] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
                index++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (false == Contains(item)) return false;

            _privateDictionary.Remove(item.Key);

            return true;
        }

        /// <summary>
        ///     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     true if the element is successfully removed; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (false == _privateDictionary.Contains(key)) return false;

            _privateDictionary.Remove(key);

            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if the key is found;
        ///     otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed
        ///     uninitialized.
        /// </param>
        /// <returns>
        ///     true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element
        ///     with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var keyExists = _privateDictionary.Contains(key);

            value = keyExists ? (TValue)_privateDictionary[key] : default(TValue);

            return keyExists;
        }

        /// <summary>
        ///     Creates a read only ordered dictionary by copying key value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>Read only ordered dictionary containing the elements in dictionary</returns>
        public static OrderedDictionary<TKey, TValue> CreateReadOnly([NotNull] IDictionary<TKey, TValue> dictionary)
            => new OrderedDictionary<TKey, TValue>(dictionary);

        /// <summary>
        ///     Copies the key value pairs of this dictionary into a new read only ordered dictionary
        /// </summary>
        /// <returns>Read only ordered dictionary containing the same elements</returns>
        public OrderedDictionary<TKey, TValue> AsReadOnly() => new OrderedDictionary<TKey, TValue>(this);
    }
}