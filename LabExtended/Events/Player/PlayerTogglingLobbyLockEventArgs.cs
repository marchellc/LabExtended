using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called before a player changes status of the lobby lock.
    /// </summary>
    public class PlayerTogglingLobbyLockEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player changing status of the lobby lock.
        /// </summary>
        public ExPlayer Player { get; }
        
        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerTogglingLobbyLockEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player toggling the lobby lock.</param>
        /// <param name="isEnabled">The lock's new status.</param>
        internal PlayerTogglingLobbyLockEventArgs(ExPlayer player, bool isEnabled)
        {
            Player = player;
            IsEnabled = isEnabled;
        }
    }
}