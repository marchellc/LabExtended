using LabExtended.API;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp173;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player looks at SCP-173.
    /// </summary>
    public class PlayerObservingScp173EventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-173.
        /// </summary>
        public ExPlayer Scp { get; }

        /// <summary>
        /// The player who looked at SCP-173.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// The SCP-173's role.
        /// </summary>
        public Scp173Role Role { get; }

        /// <summary>
        /// The SCP-173 target tracker.
        /// </summary>
        public Scp173ObserversTracker Tracker { get; }

        /// <summary>
        /// The gathered <see cref="VisionInformation"/>.
        /// </summary>
        public VisionInformation Vision { get; }

        /// <summary>
        /// Whether or not the player is looking at SCP-173.
        /// </summary>
        public bool IsLooking { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerObservingScp173EventArgs"/> instance.
        /// </summary>
        /// <param name="scp">The player playing as SCP-173.</param>
        /// <param name="player">The observing player.</param>
        /// <param name="role">SCP-173 role instance.</param>
        /// <param name="tracker">SCP-173 target tracker subroutine instance.</param>
        /// <param name="vision">Vision information data.</param>
        /// <param name="isLooking">Whether or not the player is looking.</param>
        public PlayerObservingScp173EventArgs(ExPlayer scp, ExPlayer player, Scp173Role role, 
            Scp173ObserversTracker tracker, VisionInformation vision, bool isLooking)
        {
            Scp = scp;
            Player = player;
            Role = role;
            Tracker = tracker;
            Vision = vision;
            IsLooking = isLooking;
        }
    }
}