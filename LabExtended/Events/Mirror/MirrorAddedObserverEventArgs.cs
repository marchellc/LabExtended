using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called after a new observer is added to an identity's observers list.
    /// </summary>
    public class MirrorAddedObserverEventArgs : MirrorIdentityEventArgs
    {
        /// <summary>
        /// Gets the player who was added. May be null if no instance was created for this connection.
        /// </summary>
        public ExPlayer? Observer { get; }

        /// <summary>
        /// Gets the connection of the player who was added as observer.
        /// </summary>
        public NetworkConnectionToClient Connection { get; }

        public MirrorAddedObserverEventArgs(NetworkIdentity identity, ExPlayer? observer, NetworkConnectionToClient connection) : base(identity)
        {
            Observer = observer;
            Connection = connection;
        }
    }
}