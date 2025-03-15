using LabExtended.API;

using LiteNetLib;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player is about to leave.
    /// <remarks>This event is called before any networking logic is processed, which means that you are free to interact
    /// with any Unity component.</remarks>
    /// </summary>
    public class PlayerLeavingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player who's about to leave.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets a value indicating whether or not the reason was a time out.
        /// <remarks>This happens if the server does not receive a disconnect message from the client, which means
        /// that the client's game most likely crashed.</remarks>
        /// </summary>
        public bool HasTimedOut { get; }

        /// <summary>
        /// Gets the information about this disconnect.
        /// </summary>
        public DisconnectInfo DisconnectInfo { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerLeavingEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player who left.</param>
        /// <param name="hasTimedOut">Whether or not the reason was a time out.</param>
        /// <param name="disconnectInfo">Disconnect information.</param>
        public PlayerLeavingEventArgs(ExPlayer player, bool hasTimedOut, DisconnectInfo disconnectInfo)
        {
            Player = player;
            HasTimedOut = hasTimedOut;
            DisconnectInfo = disconnectInfo;
        }
    }
}