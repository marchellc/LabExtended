using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp173;

namespace LabExtended.Events.Scp173
{
    /// <summary>
    /// Occurs when a player looks at SCP-173.
    /// </summary>
    public class PlayerObservingScp173Args : IHookEvent
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

        internal PlayerObservingScp173Args(ExPlayer scp, ExPlayer player, Scp173Role role, Scp173ObserversTracker tracker, VisionInformation vision, bool isLooking)
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