using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror destroys a network identity.
    /// </summary>
    public class MirrorDestroyingIdentityEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the destroy mode.
        /// </summary>
        public NetworkServer.DestroyMode Mode { get; }

        public MirrorDestroyingIdentityEventArgs(NetworkIdentity identity, NetworkServer.DestroyMode mode) : base(identity)
        {
            Mode = mode;
        }
    }
}