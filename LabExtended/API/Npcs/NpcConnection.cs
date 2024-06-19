using Mirror;

namespace LabExtended.API.Npcs
{
    /// <summary>
    /// A fake connection used for NPCs.
    /// </summary>
    public class NpcConnection : NetworkConnectionToClient
    {
        /// <summary>
        /// Creates a new <see cref="NpcConnection"/> instance.
        /// </summary>
        /// <param name="networkConnectionId">The connection ID to set.</param>
        public NpcConnection(int networkConnectionId) : base(networkConnectionId) { }

        /// <summary>
        /// Gets the connection's address.
        /// </summary>
        public override string address => "127.0.0.1";

        /// <summary>
        /// This method is supposed to terminate the connection, however it doesn't do anything.
        /// </summary>
        public override void Disconnect() { }

        /// <summary>
        /// This method is supposed to send data to the client, however it doesn't do anything.
        /// </summary>
        /// <param name="segment">The data to sent.</param>
        /// <param name="channelId">The channel to send it to.</param>
        public override void Send(ArraySegment<byte> segment, int channelId = 0) { }
    }
}