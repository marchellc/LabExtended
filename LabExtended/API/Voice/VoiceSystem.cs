using LabExtended.API.Voice.Profiles;
using LabExtended.API.Voice.Processing;

using LabExtended.Utilities;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events.Player;

using VoiceChat.Networking;
using VoiceChat;

using Common.Pooling.Pools;
using Common.Extensions;

namespace LabExtended.API.Voice
{
    public static class VoiceSystem
    {
        internal static bool ShowSpeakingDebug;

        static VoiceSystem()
            => UpdateEvent.OnUpdate += SpeakingWatcher;

        public static List<IVoiceProcessor> VoiceProcessors { get; } = new List<IVoiceProcessor>()
        {
            new VoicePitchProcessor()
        };

        public static event Action<VoiceMessage, ExPlayer, Dictionary<ExPlayer, VoiceChatChannel>, Action<ExPlayer, VoiceChatChannel>> OnPostProcessing;

        internal static void SetProfile(ExPlayer player, VoiceProfileBase voiceProfileBase)
        {
            if (player._voiceProfile != null)
            {
                player._voiceProfile.IsActive = false;
                player._voiceProfile.OnStopped();
                player._voiceProfile.Player = null;
            }

            if (voiceProfileBase != null)
            {
                voiceProfileBase.Player = player;
                voiceProfileBase.IsActive = true;

                player._voiceProfile = voiceProfileBase;

                voiceProfileBase.OnStarted();
            }
        }

        internal static void Receive(ref VoiceMessage message, ExPlayer speaker)
        {
            var dict = DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Rent();
            var copy = DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Rent();

            var list = ExPlayer._players;

            foreach (var player in list)
            {
                if (dict.ContainsKey(player))
                    continue;

                if (player.Role.VoiceModule is null)
                    continue;

                if (player.NetId == speaker.NetId)
                {
                    if (!player.VoiceFlags.HasFlag(VoiceFlags.CanHearSelf))
                        continue;

                    dict[player] = VoiceChatChannel.RoundSummary;
                    copy[player] = VoiceChatChannel.RoundSummary;
                    continue;
                }

                var channel = player.Role.VoiceModule.ValidateReceive(speaker.Hub, message.Channel);

                dict[player] = channel;
                copy[player] = channel;
            }

            speaker._speakingCapture.Add(message.Data);
            speaker._voiceProfile?.OnReceived(copy, (player, channel) => dict[player] = channel);

            speaker.Role.VoiceModule.CurrentChannel = message.Channel;

            for (int i = 0; i < VoiceProcessors.Count; i++)
            {
                var processor = VoiceProcessors[i];

                if (!processor.IsGloballyActive && !processor.IsActiveFor(speaker))
                    continue;

                processor.ProcessData(speaker, ref message.Data, ref message.DataLength);
            }

            if (OnPostProcessing != null)
            {
                copy.Clear();
                copy.AddRange(dict);

                OnPostProcessing(message, speaker, copy, (player, channel) => dict[player] = channel);
            }

            foreach (var player in list)
            {
                if (!dict.ContainsKey(player))
                    continue;

                var channel = dict[player];

                if (channel is VoiceChatChannel.None)
                    continue;

                message.Channel = channel;
                player.Connection.Send(message);
            }

            DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Return(copy);
            DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Return(dict);
        }

        private static void SpeakingWatcher()
        {
            for (int i = 0; i < ExPlayer._players.Count; i++)
            {
                var player = ExPlayer._players[i];

                if (player.Role.VoiceModule is null)
                    continue;

                if (player.Role.VoiceModule.ServerIsSending && !player._wasSpeaking)
                {
                    player._wasSpeaking = true;
                    player._wasSpeakingAt = DateTime.Now;
                    player._speakingCapture = ListPool<byte[]>.Shared.Rent();
                    player._voiceProfile?.OnStartedSpeaking();

                    HookManager.ExecuteCustom(new PlayerStartedSpeakingArgs(player));

                    if (ShowSpeakingDebug)
                        ExLoader.Debug("Voice API", $"Player &3{player.Name}&r (&6{player.UserId}&r) started speaking.");
                }
                else if (!player.Role.VoiceModule.ServerIsSending && player._wasSpeaking)
                {
                    var wasSpeakingAt = player._wasSpeakingAt;
                    var endedSpeakingAt = DateTime.Now;
                    var speakingDuration = endedSpeakingAt - wasSpeakingAt;
                    var capture = ListPool<byte[]>.Shared.ToArrayReturn(player._speakingCapture);

                    player._wasSpeaking = false;
                    player._wasSpeakingAt = DateTime.MinValue;
                    player._voiceProfile?.OnStoppedSpeaking(wasSpeakingAt, speakingDuration, capture);

                    HookManager.ExecuteCustom(new PlayerStoppedSpeakingArgs(player, wasSpeakingAt, speakingDuration, capture));

                    if (ShowSpeakingDebug)
                        ExLoader.Debug("Voice API", $"Player &3{player.Name}&r (&6{player.UserId}&r) stopped speaking.\n" +
                            $"- &3Started at&r &6{wasSpeakingAt}&r\n" +
                            $"- &3Spoken for&r &6{speakingDuration}&r\n" +
                            $"- &3Sent&r &6{capture.Length}&r &3packets&r.");
                }
            }
        }
    }
}