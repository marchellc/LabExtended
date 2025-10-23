using LabExtended.Core.Storage.Interfaces;

using Mirror;

using System.Collections;
using System.Collections.ObjectModel;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Represents a collection of items of type <typeparamref name="T"/> that supports read-only access, modification
    /// tracking, and common collection operations.
    /// </summary>
    /// <remarks>This class provides a wrapper around a <see cref="List{T}"/> with additional functionality,
    /// such as: <list type="bullet"> <item>Exposing the collection as a read-only view through the <see
    /// cref="Collection"/> property.</item> <item>Tracking modifications to the collection via the <see
    /// cref="StorageValue.IsDirty"/> property.</item> <item>Implementing common collection interfaces, including <see
    /// cref="IList{T}"/>, <see cref="ICollection{T}"/>, and <see cref="IReadOnlyList{T}"/>.</item> </list> The
    /// collection is not thread-safe and must be synchronized externally if accessed concurrently.</remarks>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class CollectionValue<T> : StorageValue,

        IList<T>,
        ICollection<T>,
        IReadOnlyList<T>
    {
        private List<T> collection;

        /// <summary>
        /// Gets a read-only collection of items of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>This property provides a thread-safe way to access the collection. Any attempt to
        /// modify the collection will result in a runtime exception.</remarks>
        public ReadOnlyCollection<T> Collection { get; }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => collection.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the element at the specified index in the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                var curValue = collection[index];

                if (curValue != null && curValue is IStorageValue collectionStorageValue)
                    collectionStorageValue.OnRemoved(this);

                collection[index] = value;

                if (value != null && value is IStorageValue newCollectionStorageValue)
                    newCollectionStorageValue.OnAdded(this);

                IsDirty = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValue{T}"/> class with an optional initial capacity.
        /// </summary>
        /// <param name="size">The initial number of elements that the underlying collection can contain. Defaults to 0.</param>
        public CollectionValue(int size = 0) 
            : this(new List<T>(size)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionValue{T}"/> class with the specified collection.
        /// </summary>
        /// <remarks>If the provided collection is a <see cref="List{T}"/>, it is used directly.
        /// Otherwise, the collection is copied into a new list. The resulting collection is exposed as a read-only
        /// collection through the <c>Collection</c> property.</remarks>
        /// <param name="collection">The collection of items to initialize the instance with. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is <see langword="null"/>.</exception>
        public CollectionValue(IEnumerable<T> collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            if (collection is List<T> list)
            {
                this.collection = list;
            }
            else
            {
                this.collection = new(collection);
            }

            for (var i = 0; i < this.collection.Count; i++)
            {
                var item = this.collection[i];

                if (item is IStorageValue collectionStorageValue)
                    collectionStorageValue.OnAdded(this);
            }

            Collection = this.collection.AsReadOnly();
        }

        /// <summary>
        /// Determines the zero-based index of the first occurrence of the specified item in the collection.
        /// </summary>
        /// <param name="item">The item to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item"/> in the collection,  or -1 if the
        /// item is not found.</returns>
        public int IndexOf(T item)
            => collection.IndexOf(item);

        /// <summary>
        /// Determines whether the collection contains the specified item.
        /// </summary>
        /// <remarks>The method uses the default equality comparer to determine item equality.</remarks>
        /// <param name="item">The item to locate in the collection.</param>
        /// <returns><see langword="true"/> if the specified item is found in the collection; otherwise, <see langword="false"/>.</returns>
        public bool Contains(T item)
            => collection.Contains(item);

        /// <summary>
        /// Copies the elements of the collection to the specified array, starting at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must
        /// have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
            => collection.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>The enumerator provides a simple way to iterate over the elements in the
        /// collection.</remarks>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
            => collection.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>This method is an explicit implementation of the <see
        /// cref="IEnumerable.GetEnumerator"/> method  and provides support for non-generic iteration over the
        /// collection.</remarks>
        /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => collection.GetEnumerator();

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <remarks>After the item is inserted, the collection is marked as modified.</remarks>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to insert into the collection.</param>
        public void Insert(int index, T item)
        {
            collection.Insert(index, item);

            IsDirty = true;
        }

        /// <summary>
        /// Adds the specified item to the collection and marks the collection as modified.
        /// </summary>
        /// <param name="item">The item to add to the collection. Cannot be null.</param>
        public void Add(T item)
        {
            collection.Add(item);

            if (item is IStorageValue collectionStorageValue)
                collectionStorageValue.OnAdded(this);

            IsDirty = true;
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <remarks>After a successful removal, the <c>IsDirty</c> property is set to <see
        /// langword="true"/> to indicate that the collection's state has changed.</remarks>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns><see langword="true"/> if the item was successfully removed from the collection; otherwise, <see
        /// langword="false"/> if the item was not found in the collection.</returns>
        public bool Remove(T item)
        {
            if (collection.Remove(item))
            {
                if (item is IStorageValue collectionStorageValue)
                    collectionStorageValue.OnRemoved(this);

                IsDirty = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the element at the specified index from the collection.
        /// </summary>
        /// <remarks>After the element is removed, the collection is marked as modified by setting the
        /// <see cref="StorageValue.IsDirty"/> property to <see langword="true"/>.</remarks>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < collection.Count)
            {
                var item = collection[index];

                if (item is IStorageValue collectionStorageValue)
                    collectionStorageValue.OnRemoved(this);

                collection.RemoveAt(index);

                IsDirty = true;
            }
        }

        /// <summary>
        /// Clears all items from the collection and marks the state as modified.
        /// </summary>
        /// <remarks>If the collection is already empty, this method has no effect.  After clearing, the
        /// <see cref="StorageValue.IsDirty"/> property is set to <see langword="true"/>.</remarks>
        public void Clear()
        {
            if (collection.Count > 0)
            {
                for (var i = 0; i < collection.Count; i++)
                {
                    var item = collection[i];

                    if (item is IStorageValue collectionStorageValue)
                        collectionStorageValue.OnRemoved(this);
                }

                collection.Clear();

                IsDirty = true;
            }
        }

        /// <inheritdoc/>
        public override void ReadValue(NetworkReader reader)
        {
            if (Reader<T>.read is not null)
            {
                if (collection.Count > 0)
                {
                    for (var i = 0; i < collection.Count; i++)
                    {
                        var item = collection[i];

                        if (item is IStorageValue collectionStorageValue)
                            collectionStorageValue.OnRemoved(this);
                    }

                    collection.Clear();
                }

                var count = reader.ReadInt();

                for (var i = 0; i < count; i++)
                {
                    var item = Reader<T>.read(reader);

                    if (item is IStorageValue collectionStorageValue)
                        collectionStorageValue.OnAdded(this);

                    collection.Add(item);
                }
            }
        }

        /// <inheritdoc/>
        public override void WriteValue(NetworkWriter writer)
        {
            if (Writer<T>.write is not null)
            {
                writer.WriteInt(collection.Count);

                foreach (var item in collection)
                    Writer<T>.write(writer, item);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Concat(
                "CollectionValue{", typeof(T).Name, "} [Count = ", Count.ToString(), "]");
        }
    }
}