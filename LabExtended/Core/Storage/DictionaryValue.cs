using LabExtended.Core.Storage.Interfaces;
using LabExtended.Extensions;

using Mirror;

using System.Collections;
using System.Collections.ObjectModel;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Represents a dictionary-based collection that provides additional functionality for managing key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. Keys must be unique and cannot be <see langword="null"/>.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class DictionaryValue<TKey, TValue> : StorageValue,

        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    {
        private static bool implementsStorage = typeof(TValue).InheritsType<IStorageValue>();

        private Dictionary<TKey, TValue> dictionary;

        private bool canWrite;
        private bool canRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryValue{TKey, TValue}"/> class using the specified
        /// collection of values and a function to extract keys.
        /// </summary>
        /// <param name="values">The collection of values to populate the dictionary.</param>
        /// <param name="keySelector">A function that extracts a key from each value in the collection. The key must be unique for each value;
        /// otherwise, an exception will be thrown.</param>
        public DictionaryValue(IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
            : this(values.ToDictionary(keySelector)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryValue{TKey, TValue}"/> class using the specified
        /// collection of key-value pairs.
        /// </summary>
        /// <param name="pairs">A collection of key-value pairs to populate the dictionary. Each key in the collection must be unique;
        /// otherwise, an exception will be thrown.</param>
        public DictionaryValue(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
            : this(pairs.ToDictionary()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryValue{TKey, TValue}"/> class with the specified
        /// initial size and an optional key comparer.
        /// </summary>
        /// <param name="size">The initial number of elements that the dictionary can contain.</param>
        /// <param name="comparer">An optional equality comparer to use for comparing keys. If <see langword="null"/>, the default equality
        /// comparer for the type <typeparamref name="TKey"/> is used.</param>
        public DictionaryValue(int size, IEqualityComparer<TKey>? comparer = null)
            : this(new Dictionary<TKey, TValue>(size, comparer)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryValue{TKey, TValue}"/> class with an optional key
        /// comparer.
        /// </summary>
        /// <param name="comparer">An optional equality comparer to use for comparing keys in the dictionary. If <see langword="null"/>, the
        /// default equality comparer for the key type is used.</param>
        public DictionaryValue(IEqualityComparer<TKey>? comparer = null)
            : this(new Dictionary<TKey, TValue>(comparer)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryValue{TKey, TValue}"/> class using the specified
        /// dictionary and an optional key comparer.
        /// </summary>
        /// <param name="dictionary">The dictionary whose elements are copied to the new instance. Cannot be <see langword="null"/>.</param>
        /// <param name="comparer">An optional equality comparer to use for comparing keys. If <see langword="null"/>, the default equality
        /// comparer for the key type is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dictionary"/> is <see langword="null"/>.</exception>
        public DictionaryValue(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer = null)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary is Dictionary<TKey, TValue> dict)
            {
                this.dictionary = dict;
            }
            else
            {
                this.dictionary = new(dictionary, comparer);
            }

            canWrite = Writer<TKey>.write is not null && Writer<TValue>.write is not null;
            canRead = Reader<TKey>.read is not null && Reader<TValue>.read is not null;

            if (implementsStorage)
            {
                foreach (var pair in this.dictionary)
                {
                    if (pair.Value is IStorageValue collectionStorageValue)
                    {
                        collectionStorageValue.OnAdded(this);
                    }
                }
            }

            Dictionary = new(this.dictionary);
        }

        /// <summary>
        /// Gets the underlying dictionary.
        /// </summary>
        public ReadOnlyDictionary<TKey, TValue> Dictionary { get; }

        /// <summary>
        /// Gets the amount of elements in the dictionary.
        /// </summary>
        public int Count => dictionary.Count;

        /// <summary>
        /// Gets a collection containing the keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys => dictionary.Keys;

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        /// <remarks>The order of the values in the collection corresponds to the order of their
        /// associated keys in the dictionary. Changes to the dictionary are reflected in the returned
        /// collection.</remarks>
        public ICollection<TValue> Values => dictionary.Values;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the value associated with the specified key in the dictionary.
        /// </summary>
        /// <param name="key">The key of the value to get or set. The key cannot be <see langword="null"/>.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                if (implementsStorage 
                    && dictionary.TryGetValue(key, out var existingValue)
                    && existingValue is IStorageValue existingCollectionStorageValue)
                        existingCollectionStorageValue.OnRemoved(this);              

                dictionary[key] = value;

                IsDirty = true;
            }
        }

        /// <summary>
        /// Gets an enumerable collection containing the keys in the read-only dictionary.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => dictionary.Keys;

        /// <summary>
        /// Gets an enumerable collection containing the values in the read-only dictionary.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => dictionary.Values;

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see
        /// langword="false"/>.</returns>
        public bool ContainsKey(TKey key)
            => dictionary.ContainsKey(key);

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see
        /// langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
            => dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Determines whether the collection contains the specified key-value pair.
        /// </summary>
        /// <remarks>The method checks for both the key and the value in the key-value pair. Equality is
        /// determined based on the default equality comparer for the key and value types.</remarks>
        /// <param name="item">The key-value pair to locate in the collection.</param>
        /// <returns><see langword="true"/> if the specified key-value pair is found in the collection; otherwise, <see
        /// langword="false"/>.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
            => dictionary.Contains(item);

        /// <summary>
        /// Copies the elements of the collection to the specified array, starting at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must
        /// have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the collection of key-value pairs.
        /// </summary>
        /// <remarks>The enumerator provides read-only access to the collection. It does not allow
        /// modification of the collection during enumeration.</remarks>
        /// <returns>An <see cref="IEnumerator{T}"/> for iterating through the collection of <see cref="KeyValuePair{TKey,
        /// TValue}"/> objects.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => dictionary.GetEnumerator();

        /// <summary>
        /// Adds the specified key and value to the collection.
        /// </summary>
        /// <remarks>After adding the key-value pair, the collection is marked as dirty by setting the
        /// <c>IsDirty</c> property to <see langword="true"/>.</remarks>
        /// <param name="key">The key of the element to add. Cannot be <see langword="null"/>.</param>
        /// <param name="value">The value of the element to add. Can be <see langword="null"/>.</param>
        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);

            if (implementsStorage && value is IStorageValue collectionStorageValue)
                collectionStorageValue.OnAdded(this);

            IsDirty = true;
        }

        /// <summary>
        /// Removes the element with the specified key from the collection.
        /// </summary>
        /// <remarks>After a successful removal, the <see cref="StorageValue.IsDirty"/> property is set to <see
        /// langword="true"/>.</remarks>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>. This
        /// method also returns <see langword="false"/> if the key was not found in the collection.</returns>
        public bool Remove(TKey key)
        {
            if (implementsStorage)
            {
                if (dictionary.TryGetValue(key, out var value)
                    && dictionary.Remove(key))
                {
                    if (value is IStorageValue collectionStorageValue)
                        collectionStorageValue.OnRemoved(this);

                    IsDirty = true;
                    return true;
                }
            }
            else
            {
                if (dictionary.Remove(key))
                {
                    IsDirty = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the specified key and value to the collection.
        /// </summary>
        /// <remarks>After adding the key-value pair, the collection is marked as dirty by setting the
        /// <c>IsDirty</c> property to <see langword="true"/>.</remarks>
        /// <param name="item">The pair to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item.Key, item.Value);

            if (implementsStorage && item.Value is IStorageValue collectionStorageValue)
                collectionStorageValue.OnAdded(this);

            IsDirty = true;
        }

        /// <summary>
        /// Removes the element with the specified key from the collection.
        /// </summary>
        /// <remarks>After a successful removal, the <see cref="StorageValue.IsDirty"/> property is set to <see
        /// langword="true"/>.</remarks>
        /// <param name="item">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>. This
        /// method also returns <see langword="false"/> if the key was not found in the collection.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (dictionary.Remove(item.Key))
            {
                if (implementsStorage && item.Value is IStorageValue collectionStorageValue)
                    collectionStorageValue.OnRemoved(this);

                IsDirty = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the collection of all key-value pairs.
        /// </summary>
        public void Clear()
        {
            if (dictionary.Count > 0)
            {
                if (implementsStorage)
                {
                    foreach (var pair in dictionary)
                    {
                        if (pair.Value is IStorageValue collectionStorageValue)
                        {
                            collectionStorageValue.OnRemoved(this);
                        }
                    }
                }

                dictionary.Clear();

                IsDirty = true;
            }
        }

        /// <inheritdoc/>
        public override void WriteValue(NetworkWriter writer)
        {
            if (canWrite)
            {
                writer.WriteInt(dictionary.Count);

                foreach (var pair in dictionary)
                {
                    Writer<TKey>.write(writer, pair.Key);
                    Writer<TValue>.write(writer, pair.Value);
                }
            }
        }

        /// <inheritdoc/>
        public override void ReadValue(NetworkReader reader)
        {
            if (canRead)
            {
                if (dictionary.Count > 0)
                {
                    if (implementsStorage)
                    {
                        foreach (var pair in dictionary)
                        {
                            if (pair.Value is IStorageValue collectionStorageValue)
                            {
                                collectionStorageValue.OnRemoved(this);
                            }
                        }
                    }

                    dictionary.Clear();
                }

                var count = reader.ReadInt();

                for (var i = 0; i < count; i++)
                {
                    var key = Reader<TKey>.read(reader);
                    var value = Reader<TValue>.read(reader);

                    if (value is IStorageValue collectionStorageValue)
                        collectionStorageValue.OnAdded(this);

                    dictionary[key] = value;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Concat(
                "DictionaryValue{", typeof(TKey).Name, ",", typeof(TValue).Name, "} (", dictionary.Count, ")");
        }
    }
}