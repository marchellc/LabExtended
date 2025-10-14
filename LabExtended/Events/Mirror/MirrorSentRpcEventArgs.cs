using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called after an RPC is sent to a player.
    /// </summary>
    public class MirrorSentRpcEventArgs : MirrorIdentityEventArgs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorSentRpcEventArgs"/> class.
        /// </summary>
        internal static MirrorSentRpcEventArgs Singleton { get; } = new();

        /// <summary>
        /// Gets the player who received the RPC.
        /// </summary>
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets the writer which contains the data of the RPC.
        /// </summary>
        public NetworkWriter Writer { get; internal set; }

        /// <summary>
        /// Gets the behaviour which sent the RPC.
        /// </summary>
        public NetworkBehaviour Behaviour { get; internal set; }

        /// <summary>
        /// Gets the hash of the RPC method name.
        /// </summary>
        public int RpcHash { get; internal set; }

        /// <summary>
        /// Gets the full name of the RPC method.
        /// </summary>
        public string RpcName { get; internal set; }
    }
}