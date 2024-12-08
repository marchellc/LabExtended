using LabExtended.API;

namespace LabExtended.Events.Player
{
    public class PlayerOpenedRemoteAdminArgs
    {
        public ExPlayer Player { get; }

        internal PlayerOpenedRemoteAdminArgs(ExPlayer player)
            => Player = player;
    }
}