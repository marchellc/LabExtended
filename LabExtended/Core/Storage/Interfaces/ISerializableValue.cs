using Mirror;

namespace LabExtended.Core.Storage.Interfaces
{
    /// <summary>
    /// Defines methods for serializing and deserializing object state.
    /// </summary>
    public interface ISerializableValue
    {
        /// <summary>
        /// Serializes the current object state into the specified <see cref="NetworkWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="NetworkWriter"/> to which the object state will be serialized. Cannot be null.</param>
        void Serialize(NetworkWriter writer);

        /// <summary>
        /// Deserializes data from the specified <see cref="NetworkReader"/> into the current object.
        /// </summary>
        /// <param name="reader">The <see cref="NetworkReader"/> instance from which to read the serialized data. Cannot be null.</param>
        void Deserialize(NetworkReader reader);
    }
}