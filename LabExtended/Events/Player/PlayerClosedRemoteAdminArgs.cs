using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Player
{
    public class PlayerClosedRemoteAdminArgs : IHookEvent
    {
        public ExPlayer Player { get; }

        internal PlayerClosedRemoteAdminArgs(ExPlayer player)
            => Player = player;
    }
}