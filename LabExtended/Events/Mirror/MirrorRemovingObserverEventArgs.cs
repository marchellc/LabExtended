using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before an observer is removed from an identity's observers list.
    /// </summary>
    public class MirrorRemovingObserverEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorRemovingObserverEventArgs"/> class.
        /// </summary>
        internal static MirrorRemovingObserverEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the player who should be removed. May be null if no instance was created for this connection.
        /// </summary>
        public ExPlayer? Observer { get; internal set; }

        /// <summary>
        /// Gets the connection of the player to be removed as observer.
        /// </summary>
        public NetworkConnection Connection { get; internal set; }
    }
}