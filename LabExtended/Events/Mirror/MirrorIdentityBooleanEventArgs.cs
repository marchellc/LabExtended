using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Base class for Mirror events with a network identity.
    /// </summary>
    public class MirrorIdentityBooleanEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorIdentityBooleanEventArgs"/> class.
        /// </summary>
        internal static MirrorIdentityBooleanEventArgs Instance { get; } = new();

        /// <summary>
        /// Gets the targeted network identity.
        /// </summary>
        public NetworkIdentity Identity { get; internal set; }
    }
} 