using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called after the player stops sending player list requests, which means that they closed their Remote Admin panel.
    /// <remarks>This is not really accurate and contains about a second of delay after the actual closing happens.</remarks>
    /// </summary>
    public class PlayerClosedRemoteAdminEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player who closed their Remote Admin panel.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerClosedRemoteAdminEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player.</param>
        public PlayerClosedRemoteAdminEventArgs(ExPlayer player)
            => Player = player;
    }
}