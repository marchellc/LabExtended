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
        public NetworkBehaviour Behaviour { get; }

        /// <summary>
        /// Gets the hash of the RPC method name.
        /// </summary>
        public int RpcHash { get; }

        /// <summary>
        /// Gets the full name of the RPC method.
        /// </summary>
        public string RpcName { get; }

        public MirrorSentRpcEventArgs(ExPlayer player, NetworkWriter writer, NetworkBehaviour behaviour, int rpcHash, string rpcName) : base(behaviour.netIdentity)
        {
            Player = player;
            Writer = writer;
            Behaviour = behaviour;
            RpcHash = rpcHash;
            RpcName = rpcName;
        }
    }
}