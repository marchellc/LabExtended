using Common.Values;

using Mirror;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A wrapping class for networked objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkedWrapper<T> : NetworkedObject, IWrapper<T>
        where T : NetworkBehaviour
    {
        /// <summary>
        /// Creates a new instance of this wrapper.
        /// </summary>
        /// <param name="baseValue">The wrapped value.</param>
        public NetworkedWrapper(T baseValue)
            => Base = baseValue;

        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        public T Base { get; }

        /// <summary>
        /// Gets the wrapped object's network identity.
        /// </summary>
        public override NetworkIdentity Identity => Base.netIdentity;
    }
}