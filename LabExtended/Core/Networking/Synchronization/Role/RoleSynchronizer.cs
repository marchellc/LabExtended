using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Ticking;

using PlayerRoles;

namespace LabExtended.Core.Networking.Synchronization.Role
{
    public static class RoleSynchronizer
    {
        public static void Synchronize()
        {
            if (ExPlayer._allPlayers.Count < 1)
                return;

            for (int i = 0; i < ExPlayer._allPlayers.Count; i++)
            {
                var player = ExPlayer._allPlayers[i];

                if (player is null || !player)
                    continue;

                for (int x = 0; x < ExPlayer._allPlayers.Count; x++)
                {
                    var other = ExPlayer._allPlayers[x];
                    var role = player.Role.Type;

                    if (other is null || !other)
                        continue;

                    if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                        role = obfuscatedRole.GetRoleForUser(other.Hub);

                    if (player.Role.FakedList.GlobalValue != RoleTypeId.None)
                        role = player.Role.FakedList.GlobalValue;

                    if (player.Role.FakedList.TryGetValue(other, out var fakedRole))
                        role = fakedRole;

                    if (!other.Role.IsAlive && !player.Switches.IsVisibleInSpectatorList)
                        role = RoleTypeId.Spectator;

                    if (player._sentRoles.TryGetValue(other.PlayerId, out var sentRole) && sentRole == role)
                        continue;

                    player._sentRoles[other.PlayerId] = role;
                    other.Connection.Send(new RoleSyncInfo(player.Hub, role, other.Hub));
                }
            }
        }

        [LoaderInitialize(1)]
        internal static void InternalLoad()
            => ApiLoader.ApiConfig.TickSection.GetCustomOrDefault("RoleSync", TickDistribution.UnityTick).CreateHandle(TickDistribution.CreateWith(Synchronize, new TickOptions(TickFlags.Separate)));
    }
}