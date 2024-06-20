using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// An event that gets called when a player stops speaking.
    /// </summary>
    public class PlayerStoppedSpeakingArgs : IHookEvent
    {
        /// <summary>
        /// The player who stopped speaking.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// The time when the player started speaking.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// How long the player was speaking.
        /// </summary>
        public TimeSpan SpeakingFor { get; }

        /// <summary>
        /// An array of captured voice packets.
        /// </summary>
        public byte[][] Packets { get; }

        internal PlayerStoppedSpeakingArgs(ExPlayer player, DateTime startedAt, TimeSpan speakingFor, byte[][] packets)
        {
            Player = player;
            StartedAt = startedAt;
            SpeakingFor = speakingFor;
            Packets = packets;
        }
    }
}