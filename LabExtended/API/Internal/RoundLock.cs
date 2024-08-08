using Footprinting;

using CentralAuth;

namespace LabExtended.API.Internal
{
    /// <summary>
    /// Used for managing round lock history.
    /// </summary>
    public struct RoundLock
    {
        /// <summary>
        /// The player who enabled the round lock.
        /// </summary>
        public readonly ExPlayer EnabledBy;

        /// <summary>
        /// The player's <see cref="Footprint"/> in case the player left.
        /// </summary>
        public readonly Footprint EnabledByFootprint;

        /// <summary>
        /// The time of the round lock being enabled.
        /// </summary>
        public readonly DateTime EnabledAt;

        /// <summary>
        /// Gets the amount of time that has passed.
        /// </summary>
        public TimeSpan TimeSince => DateTime.Now - EnabledAt;

        /// <summary>
        /// Gets a value indicating whether or not the round lock was enabled by the server (aka by the API).
        /// </summary>
        public bool IsHost => EnabledBy?.IsServer ?? EnabledByFootprint.Hub.Mode != ClientInstanceMode.ReadyClient;

        /// <summary>
        /// Gets a value indicating whether or not the player who enabled the round lock has left the server.
        /// </summary>
        public bool HasLeft => !EnabledBy;

        /// <summary>
        /// Creates a new <see cref="RoundLock"/> instance.
        /// </summary>
        /// <param name="enabledBy">The player who enabled the round lock.</param>
        public RoundLock(ExPlayer enabledBy)
        {
            EnabledBy = enabledBy;
            EnabledByFootprint = enabledBy.Footprint;

            EnabledAt = DateTime.Now;
        }
    }
}