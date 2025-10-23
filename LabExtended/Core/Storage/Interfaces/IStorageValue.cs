namespace LabExtended.Core.Storage.Interfaces
{
    /// <summary>
    /// Represents a storage value that can be serialized and deserialized, with support for parent-child relationships.
    /// </summary>
    /// <remarks>This interface extends <see cref="ISerializableValue"/> to include functionality for managing
    /// hierarchical storage values. Implementations should handle the addition and removal of storage values in
    /// relation to a parent <see cref="StorageValue"/>.</remarks>
    public interface IStorageValue : ISerializableValue
    {
        /// <summary>
        /// Gets or sets the parent storage value.
        /// </summary>
        StorageValue ParentValue { get; set; }

        /// <summary>
        /// Handles the event when a new <see cref="StorageValue"/> is added.
        /// </summary>
        /// <remarks>This method is invoked to perform any necessary actions when a new storage value is
        /// added to the parent. Ensure that <paramref name="parentValue"/> is not null before calling this
        /// method.</remarks>
        /// <param name="parentValue">The parent <see cref="StorageValue"/> to which the new value is added. Cannot be null.</param>
        void OnAdded(StorageValue parentValue);

        /// <summary>
        /// Handles the event when a storage value is removed from its parent.
        /// </summary>
        /// <remarks>This method is invoked to perform any necessary cleanup or updates when a storage
        /// value is removed. Ensure that <paramref name="parentValue"/> is not null before calling this
        /// method.</remarks>
        /// <param name="parentValue">The parent <see cref="StorageValue"/> from which the value is removed. Cannot be null.</param>
        void OnRemoved(StorageValue parentValue);
    }
}