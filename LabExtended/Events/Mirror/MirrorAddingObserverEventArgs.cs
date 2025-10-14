using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before a new observer is added to an identity's observers list.
    /// </summary>
    public class MirrorAddingObserverEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorAddingObserverEventArgs"/> class.
        /// </summary>
        internal static MirrorAddingObserverEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the player who should be added. May be null if no instance was created for this connection.
        /// </summary>
        public ExPlayer? Observer { get; internal set; }

        /// <summary>
        /// Gets the connection of the player to be added as observer.
        /// </summary>
        public NetworkConnectionToClient Connection { get; internal set; }
    }
}