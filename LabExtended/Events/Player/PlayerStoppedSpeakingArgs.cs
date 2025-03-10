using LabExtended.API;

using UnityEngine;

using VoiceChat.Networking;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// An event that gets called when a player stops speaking.
    /// </summary>
    public class PlayerStoppedSpeakingArgs
    {
        private DateTime? startTime;
        private TimeSpan? duration;
        
        /// <summary>
        /// The player who stopped speaking.
        /// </summary>
        public ExPlayer Player { get; }
        
        /// <summary>
        /// The time when the player started speaking.
        /// </summary>
        public float StartTime { get; }

        /// <summary>
        /// The time when the player started speaking.
        /// </summary>
        public DateTime StartedAt
        {
            get
            {
                if (!startTime.HasValue)
                    startTime = DateTime.Now.Subtract(SpeakingFor);

                return startTime.Value;
            }
        }

        /// <summary>
        /// How long the player was speaking.
        /// </summary>
        public TimeSpan SpeakingFor
        {
            get
            {
                if (!duration.HasValue)
                    duration = TimeSpan.FromSeconds(Time.realtimeSinceStartup - StartTime);

                return duration.Value;
            }
        }

        /// <summary>
        /// An array of captured voice packets.
        /// </summary>
        public IReadOnlyDictionary<DateTime, VoiceMessage>? Packets { get; }

        internal PlayerStoppedSpeakingArgs(ExPlayer player, float startTime, IReadOnlyDictionary<DateTime, VoiceMessage>? packets)
        {
            Player = player;
            Packets = packets;
            StartTime = startTime;
        }
    }
}