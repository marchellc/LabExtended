using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before a behaviour writes it's dirty properties.
    /// </summary>
    public class MirrorSerializingBehaviourEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorSerializingBehaviourEventArgs"/> class.
        /// </summary>
        internal static MirrorSerializingBehaviourEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the behaviour being serialized.
        /// </summary>
        public NetworkBehaviour Behaviour { get; internal set; }
    
        /// <summary>
        /// Gets or sets the network writer containing the serialized data.
        /// </summary>
        public NetworkWriter Writer { get; set; }
    }
}