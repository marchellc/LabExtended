using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called after a behaviour writes it's dirty properties.
    /// </summary>
    public class MirrorSerializedBehaviourEventArgs : MirrorIdentityEventArgs
    {
        /// <summary>
        /// Gets the behaviour that was serialized.
        /// </summary>
        public NetworkBehaviour Behaviour { get; internal set; }

        /// <summary>
        /// Gets the network writer containing the serialized data.
        /// </summary>
        public NetworkWriter Writer { get; internal set; }

        /// <summary>
        /// Whether or not the behaviour's dirty bits should be reset to zero.
        /// </summary>
        public bool ResetBits { get; set; }

        public MirrorSerializedBehaviourEventArgs(NetworkBehaviour behaviour, NetworkWriter writer, bool resetBits) : base(behaviour.netIdentity)
        {
            Behaviour = behaviour;
            Writer = writer;
            ResetBits = resetBits;
        }
    }
}