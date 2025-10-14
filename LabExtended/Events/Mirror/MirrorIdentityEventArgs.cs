using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Base class for Mirror events with a network identity.
    /// </summary>
    public class MirrorIdentityEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorIdentityEventArgs"/> class.
        /// </summary>
        internal static MirrorIdentityEventArgs Instance { get; } = new();

        /// <summary>
        /// Gets the targeted network identity.
        /// </summary>
        public NetworkIdentity Identity { get; internal set; }
    }
}