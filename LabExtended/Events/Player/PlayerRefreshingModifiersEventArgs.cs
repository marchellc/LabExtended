using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player's movement limit modifiers are refreshed.
    /// </summary>
    public class PlayerRefreshingModifiersEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player who's modifiers are being refreshed.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets or sets the new value of the stamina usage multiplier.
        /// </summary>
        public float StaminaUsageMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the new value of the movement speed multiplier.
        /// </summary>
        public float MovementSpeedMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the new value of the movement speed limiter.
        /// </summary>
        public float MovementSpeedLimiter { get; set; }

        /// <summary>
        /// Initializes a new instance of the PlayerRefreshingModifiersEventArgs class with the specified player and
        /// modifier values before and after refresh.
        /// </summary>
        /// <param name="player">The player whose movement and stamina modifiers are being refreshed. Cannot be null.</param>
        /// <param name="staminaUsage">The new multiplier to be applied to the player's stamina usage after the refresh.</param>
        /// <param name="movementSpeed">The new multiplier to be applied to the player's movement speed after the refresh.</param>
        /// <param name="speedLimit">The new value limiting the player's movement speed after the refresh.</param>
        public PlayerRefreshingModifiersEventArgs(ExPlayer player, float staminaUsage, float movementSpeed, float speedLimit)
        {
            Player = player;

            StaminaUsageMultiplier = staminaUsage;
            MovementSpeedMultiplier = movementSpeed;
            MovementSpeedLimiter = speedLimit;
        }
    }
}