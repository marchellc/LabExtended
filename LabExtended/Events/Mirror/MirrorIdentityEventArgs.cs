using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Base class for Mirror events with a network identity.
    /// </summary>
    public abstract class MirrorIdentityEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the targeted network identity.
        /// </summary>
        public NetworkIdentity Identity { get; }

        public MirrorIdentityEventArgs(NetworkIdentity identity)
        {
            Identity = identity;
        }
    }
}