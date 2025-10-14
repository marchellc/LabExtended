using Mirror;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Base class for storage values.
    /// </summary>
    public class StorageValue
    {
        internal int dirtyRetries = 0;

        /// <summary>
        /// Gets the dirty bitmask of this storage value.
        /// </summary>
        public ulong DirtyBit { get; internal set; }

        /// <summary>
        /// Gets the name of this storage value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the path to the file of this storage value.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Gets the full path of the value, combining the storage name and the current name.
        /// </summary>
        public string ValuePath
        {
            get
            {
                if (Storage is null)
                    return "(null)";

                return $"{Storage.Name}/{Name}";
            }
        }

        /// <summary>
        /// Gets the parent storage instance of this value.
        /// </summary>
        public StorageInstance Storage { get; internal set; }

        /// <summary>
        /// Gets whether this value is dirty and needs to be saved.
        /// </summary>
        public bool IsDirty => Storage != null && (Storage.dirtyBits & DirtyBit) == DirtyBit;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageValue"/> class.
        /// </summary>
        public StorageValue()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageValue"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name associated with the storage value. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or an empty string.</exception>
        public StorageValue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        /// <summary>
        /// Applies the default value to this instance.
        /// </summary>
        public virtual void ApplyDefault()
        {

        }

        /// <summary>
        /// Gets called once the value of this instance is changed by another server.
        /// </summary>
        public virtual void OnChanged()
        {
            
        }

        /// <summary>
        /// Gets called once the value of this file is initially loaded.
        /// </summary>
        public virtual void OnLoaded()
        {

        }

        /// <summary>
        /// Called when the object is added to its parent storage instance.
        /// </summary>
        public virtual void OnAdded()
        {

        }

        /// <summary>
        /// Gets called once the parent storage instance is destroyed.
        /// </summary>
        public virtual void OnDestroyed()
        {
            
        }

        /// <summary>
        /// Writes the value to the given <see cref="NetworkWriter"/>.
        /// </summary>
        /// <param name="writer"></param>
        public virtual void WriteValue(NetworkWriter writer)
        {

        }

        /// <summary>
        /// Reads the value from the given <see cref="NetworkReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void ReadValue(NetworkReader reader)
        {

        }

        /// <summary>
        /// Marks the object as dirty, indicating that it has been modified and requires further processing.
        /// </summary>
        /// <remarks>This method sets the appropriate dirty bit in the associated storage if the object is
        /// not already marked as dirty. No action is taken if the storage is null or the object is already marked as
        /// dirty.</remarks>
        public void MakeDirty()
        {
            if (Storage is null || IsDirty)
                return;

            Storage.dirtyBits |= DirtyBit;
        }
    }
}