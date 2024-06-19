using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerStoppedSpeakingArgs : HookEvent
    {
        public ExPlayer Player { get; }

        public DateTime StartedAt { get; }
        public TimeSpan SpeakingFor { get; }

        public byte[][] Packets { get; }

        public PlayerStoppedSpeakingArgs(ExPlayer player, DateTime startedAt, TimeSpan speakingFor, byte[][] packets)
        {
            Player = player;
            StartedAt = startedAt;
            SpeakingFor = speakingFor;
            Packets = packets;
        }
    }
}