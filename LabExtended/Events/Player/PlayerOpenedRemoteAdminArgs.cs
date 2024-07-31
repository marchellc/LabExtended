using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Player
{
    public class PlayerOpenedRemoteAdminArgs : IHookEvent
    {
        public ExPlayer Player { get; }

        internal PlayerOpenedRemoteAdminArgs(ExPlayer player)
            => Player = player;
    }
}