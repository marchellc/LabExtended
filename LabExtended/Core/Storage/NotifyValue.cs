using LabExtended.Core.Storage.Interfaces;

using Mirror;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Represents an abstract base class for values that notify changes within a collection.
    /// </summary>
    /// <remarks>This class provides a mechanism to handle the addition and removal of values in a collection,
    /// allowing derived classes to implement serialization and deserialization logic.</remarks>
    public abstract class NotifyValue : IStorageValue
    {
        /// <inheritdoc/>
        public StorageValue ParentValue { get; set; }

        /// <summary>
        /// Notifies the parent object that a change has occurred.
        /// </summary>
        /// <remarks>Sets the <see cref="StorageValue.IsDirty"/> property to <see langword="true"/> if <see
        /// cref="ParentValue"/> is not <see langword="null"/>. This indicates that the parent object has been modified
        /// and may require further processing or saving.</remarks>
        public void Notify()
        {
            if (ParentValue is null)
                return;

            ParentValue.IsDirty = true;
        }

        /// <inheritdoc/>
        public void OnAdded(StorageValue parentValue)
            => ParentValue = parentValue;

        /// <inheritdoc/>
        public void OnRemoved(StorageValue parentValue)
            => ParentValue = null!;

        /// <inheritdoc/>
        public abstract void Deserialize(NetworkReader reader);

        /// <inheritdoc/>
        public abstract void Serialize(NetworkWriter writer);
    }
}