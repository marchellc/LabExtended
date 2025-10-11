using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before an RPC is sent to a player.
    /// </summary>
    public class MirrorSendingRpcEventArgs : MirrorIdentityBooleanEventArgs
    {
        /// <summary>
        /// Gets the player who will receive the RPC.
        /// </summary>
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets or sets the writer which contains the data of the RPC.
        /// </summary>
        public NetworkWriter Writer { get; set; }

        /// <summary>
        /// Gets the behaviour sending the RPC.
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

        public MirrorSendingRpcEventArgs(ExPlayer player, NetworkWriter writer, NetworkBehaviour behaviour, int rpcHash, string rpcName) : base(behaviour.netIdentity)
        {
            Player = player;
            Writer = writer;
            Behaviour = behaviour;
            RpcHash = rpcHash;
            RpcName = rpcName;
        }
    }
}