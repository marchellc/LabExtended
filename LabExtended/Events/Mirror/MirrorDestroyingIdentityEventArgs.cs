using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror destroys a network identity.
    /// </summary>
    public class MirrorDestroyingIdentityEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorDestroyingIdentityEventArgs"/> class.
        /// </summary>
        internal static MirrorDestroyingIdentityEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the destroy mode.
        /// </summary>
        public NetworkServer.DestroyMode Mode { get; internal set; }
    }
}