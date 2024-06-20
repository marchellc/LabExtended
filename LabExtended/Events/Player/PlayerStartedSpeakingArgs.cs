using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// An event that gets called when a player starts speaking.
    /// </summary>
    public class PlayerStartedSpeakingArgs : IHookEvent
    {
        /// <summary>
        /// The player who started speaking.
        /// </summary>
        public ExPlayer Player { get; }

        internal PlayerStartedSpeakingArgs(ExPlayer player)
            => Player = player;
    }
}