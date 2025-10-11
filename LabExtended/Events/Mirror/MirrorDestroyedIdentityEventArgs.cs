using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror destroys a network identity.
    /// </summary>
    public class MirrorDestroyedIdentityEventArgs : MirrorIdentityEventArgs    
    {
        /// <summary>
        /// Gets the destroy mode.
        /// </summary>
        public NetworkServer.DestroyMode Mode { get; }

        public MirrorDestroyedIdentityEventArgs(NetworkIdentity identity, NetworkServer.DestroyMode mode) : base(identity)
        {
            Mode = mode;
        }
    }
}