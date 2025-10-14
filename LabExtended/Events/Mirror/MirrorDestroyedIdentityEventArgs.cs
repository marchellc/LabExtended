using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror destroys a network identity.
    /// </summary>
    public class MirrorDestroyedIdentityEventArgs : MirrorIdentityEventArgs    
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorDestroyedIdentityEventArgs"/> class.
        /// </summary>
        internal static MirrorDestroyedIdentityEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the destroy mode.
        /// </summary>
        public NetworkServer.DestroyMode Mode { get; internal set; }
    }
}