using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerStartedSpeakingArgs : HookEvent
    {
        public ExPlayer Player { get; }

        public PlayerStartedSpeakingArgs(ExPlayer player)
            => Player = player;
    }
}