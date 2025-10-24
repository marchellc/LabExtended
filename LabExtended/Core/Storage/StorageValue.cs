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
        /// Gets or sets whether this value is dirty and needs to be saved.
        /// </summary>
        public bool IsDirty
        {
            get => field;
            set
            {
                if (value != field)
                {
                    if (value)
                    {
                        LastChangeTime = UnityEngine.Time.realtimeSinceStartup;
                    }
                    else
                    {
                        dirtyRetries = 0;

                        LastSaveTime = UnityEngine.Time.realtimeSinceStartup;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the time, in seconds, of the most recent change (from <see cref="UnityEngine.Time.realtimeSinceStartup"/>).
        /// </summary>
        public float LastChangeTime { get; internal set; }

        /// <summary>
        /// Gets the time, in seconds, since the last save (from <see cref="UnityEngine.Time.realtimeSinceStartup"/>).
        /// </summary>
        public float LastSaveTime { get; internal set; }

        /// <summary>
        /// Gets the number of seconds that have elapsed since the last change occurred.
        /// </summary>
        public float SecondsSinceChange => UnityEngine.Time.realtimeSinceStartup - LastChangeTime;

        /// <summary>
        /// Gets the number of seconds that have elapsed since the value was last saved.
        /// </summary>
        public float SecondsSinceSave => UnityEngine.Time.realtimeSinceStartup - LastSaveTime;

        /// <summary>
        /// Gets the number of seconds that have elapsed since the value was last marked as dirty.
        /// </summary>
        public float SecondsDirty => IsDirty ? SecondsSinceChange : 0f;

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
        /// Sets the specified field to a new value and marks the object as modified if the value changes.
        /// </summary>
        /// <remarks>The method updates the field only if the new value is different from the current
        /// value, considering both null and non-null comparisons. If the field is updated, the object is marked as
        /// modified by setting the <c>IsDirty</c> property to <see langword="true"/>.</remarks>
        /// <typeparam name="T">The type of the field and value.</typeparam>
        /// <param name="field">A reference to the field to be updated.</param>
        /// <param name="value">The new value to assign to the field.</param>
        /// <returns><see langword="true"/> if the field was updated and the object was marked as modified; otherwise, <see
        /// langword="false"/> if the field value remains unchanged.</returns>
        public bool SetField<T>(ref T field, T value)
        {
            if (field == null && value == null)
                return false;

            if ((field is null && value != null)
                || (field != null && value == null)
                || (field != null && value != null && !field.Equals(value)))
            {
                field = value;

                IsDirty = true;
                return true;
            }

            return false;
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
        /// Gets called when the dirty value is saved.
        /// </summary>
        public virtual void OnSaved()
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
    }
}