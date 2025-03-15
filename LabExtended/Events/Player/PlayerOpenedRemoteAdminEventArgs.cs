using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player opens their Remote Admin panel.
    /// <remarks>This event relies on the player sending a player list request, which is not always
    /// a 100% accurate.</remarks>
    /// </summary>
    public class PlayerOpenedRemoteAdminEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player who opened their Remote Admin panel.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerOpenedRemoteAdminEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player.</param>
        public PlayerOpenedRemoteAdminEventArgs(ExPlayer player)
            => Player = player;
    }
}