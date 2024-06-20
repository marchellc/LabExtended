using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

using LiteNetLib;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Occurs when a player is about to leave. <b>This event is called BEFORE any Mirror logic is processed, so you can use all Unity stuff.</b>
    /// </summary>
    public class PlayerLeavingArgs : IHookEvent
    {
        /// <summary>
        /// Gets the player who's about to leave.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets a value indicating whether or not the reason was a time out (likely a game crash or a network outage, however this may not be accurate).
        /// </summary>
        public bool HasTimedOut { get; }

        /// <summary>
        /// Gets the disconnect info associated with this event.
        /// </summary>
        public DisconnectInfo DisconnectInfo { get; }

        internal PlayerLeavingArgs(ExPlayer player, bool hasTimedOut, DisconnectInfo disconnectInfo)
        {
            Player = player;
            HasTimedOut = hasTimedOut;
            DisconnectInfo = disconnectInfo;
        }
    }
}