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
        /// Gets the player who should be removed. May be null if no instance was created for this connection.
        /// </summary>
        public ExPlayer? Observer { get; }

        /// <summary>
        /// Gets the connection of the player to be removed as observer.
        /// </summary>
        public NetworkConnection Connection { get; }

        public MirrorRemovingObserverEventArgs(NetworkIdentity identity, ExPlayer? observer, NetworkConnection connection) : base(identity)
        {
            Observer = observer;
            Connection = connection;
        }
    }
}