using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// An event that gets called when a player starts speaking.
    /// </summary>
    public class PlayerStartedSpeakingArgs
    {
        /// <summary>
        /// The player who started speaking.
        /// </summary>
        public ExPlayer Player { get; }

        internal PlayerStartedSpeakingArgs(ExPlayer player)
            => Player = player;
    }
}