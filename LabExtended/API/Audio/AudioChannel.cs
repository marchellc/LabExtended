using VoiceChat;

namespace LabExtended.API.Audio
{
    public class AudioChannel
    {
        public struct TargetChannel
        {
            public readonly ExPlayer Speaker;
            public readonly ExPlayer Receiver;

            public readonly VoiceChatChannel Channel;

            internal TargetChannel(ExPlayer speaker, ExPlayer receiver, VoiceChatChannel channel)
            {
                Speaker = speaker;
                Receiver = receiver;
                Channel = channel;
            }
        }

        public Dictionary<ExPlayer, TargetChannel> Channels { get; } = new Dictionary<ExPlayer, TargetChannel>();

        public TargetChannel? Get(ExPlayer receiver)
        {
            if (Channels.TryGetValue(receiver, out var channel))
                return channel;

            return null;
        }

        public void Set(ExPlayer receiver, ExPlayer speaker, VoiceChatChannel channel)
            => Channels[receiver] = new TargetChannel(speaker, receiver, channel);

        public void Set(ExPlayer speaker, VoiceChatChannel channel, bool includeSpeaker = true)
        {
            foreach (var player in ExPlayer.Players)
            {
                if (player == speaker && !includeSpeaker)
                    continue;

                Channels[player] = new TargetChannel(speaker, player, channel);
            }
        }

        public void Set(Func<ExPlayer, bool> predicate, ExPlayer speaker, VoiceChatChannel channel)
        {
            foreach (var player in ExPlayer.Get(predicate))
                Channels[player] = new TargetChannel(speaker, player, channel);
        }
    }
}