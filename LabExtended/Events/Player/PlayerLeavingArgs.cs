using LabExtended.API;
using LabExtended.Core.Events;
using LiteNetLib;

namespace LabExtended.Events.Player
{
    public class PlayerLeavingArgs : HookEvent
    {
        public ExPlayer Player { get; }

        public bool HasTimedOut { get; }

        public DisconnectInfo DisconnectInfo { get; }

        public PlayerLeavingArgs(ExPlayer player, bool hasTimedOut, DisconnectInfo disconnectInfo)
        {
            Player = player;
            HasTimedOut = hasTimedOut;
            DisconnectInfo = disconnectInfo;
        }
    }
}