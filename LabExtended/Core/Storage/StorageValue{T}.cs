using Mirror;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Represents a strongly-typed storage value that supports default values and serialization.
    /// </summary>
    /// <remarks>This class provides functionality for managing a value of type <typeparamref name="T"/> with
    /// optional default value support. It also includes methods for reading and writing the value.</remarks>
    /// <typeparam name="T">The type of the value being stored.</typeparam>
    public class StorageValue<T> : StorageValue
    {
        private T value;
        private T? defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageValue{T}"/> class with an optional default value.
        /// </summary>
        /// <param name="defaultValue">The default value to initialize the storage with. If not provided, the default value for the type
        /// <typeparamref name="T"/> is used.</param>
        public StorageValue(T? defaultValue = default) : base()
            => this.defaultValue = defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageValue{T}"/> class with the specified name and an
        /// optional default value.
        /// </summary>
        /// <remarks>The <paramref name="name"/> parameter uniquely identifies the storage value, and the
        /// <paramref name="defaultValue"/> parameter provides a fallback value when no other value is
        /// assigned.</remarks>
        /// <param name="name">The name associated with the storage value. This cannot be null or empty.</param>
        /// <param name="defaultValue">The default value to be used if no value is explicitly set. The default is <see langword="default"/> for the
        /// type <typeparamref name="T"/>.</param>
        public StorageValue(string name, T? defaultValue = default) : base(name)
            => this.defaultValue = defaultValue;

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        public T Value
        {
            get => value;
            set
            {
                this.value = value;

                MakeDirty();
            }
        }

        /// <inheritdoc/>
        public override void ApplyDefault()
        {
            Value = defaultValue!;
        }

        /// <inheritdoc/>
        public override void WriteValue(NetworkWriter writer)
        {
            Writer<T>.write?.Invoke(writer, Value);
        }

        /// <inheritdoc/>
        public override void ReadValue(NetworkReader reader)
        {
            if (Reader<T>.read != null)
                value = Reader<T>.read(reader);
        }
    }
}