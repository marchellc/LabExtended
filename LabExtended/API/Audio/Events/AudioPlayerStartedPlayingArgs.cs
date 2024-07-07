using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.API.Audio.Events
{
    public class AudioPlayerStartedPlayingArgs : IHookEvent
    {
        public AudioPlayer Player { get; }

        internal AudioPlayerStartedPlayingArgs(AudioPlayer player)
            => Player = player;
    }
}