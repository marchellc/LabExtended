using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called before a player changes status of the round lock.
    /// </summary>
    public class PlayerTogglingRoundLockEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player changing status of the round lock.
        /// </summary>
        public ExPlayer Player { get; }
        
        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerTogglingRoundLockEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player toggling the round lock.</param>
        /// <param name="isEnabled">The lock's new status.</param>
        internal PlayerTogglingRoundLockEventArgs(ExPlayer player, bool isEnabled)
        {
            Player = player;
            IsEnabled = isEnabled;
        }
    }
}