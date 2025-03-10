using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Utilities.Unity;

using PlayerRoles;

using UnityEngine.PlayerLoop;

namespace LabExtended.Core.Networking.Synchronization.Role
{
    public static class RoleSynchronizer
    {
        public struct RoleSyncUpdateLoop { }
        
        private static void OnUpdate()
        {
            if (ExPlayer.AllPlayers.Count < 1)
                return;

            for (int i = 0; i < ExPlayer.AllPlayers.Count; i++)
            {
                var player = ExPlayer.AllPlayers[i];

                if (player is null || !player || player.Role is null || player.SentRoles is null)
                    continue;

                for (int x = 0; x < ExPlayer.AllPlayers.Count; x++)
                {
                    var other = ExPlayer.AllPlayers[x];
                    var role = player.Role.Type;

                    if (other is null || !other || other.Role is null)
                        continue;

                    if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                        role = obfuscatedRole.GetRoleForUser(other.ReferenceHub);

                    if (player.Role.FakedList.GlobalValue != RoleTypeId.None)
                        role = player.Role.FakedList.GlobalValue;

                    if (player.Role.FakedList.TryGetValue(other, out var fakedRole))
                        role = fakedRole;

                    if (!other.Role.IsAlive && !player.Toggles.IsVisibleInSpectatorList)
                        role = RoleTypeId.Spectator;

                    if (player.SentRoles.TryGetValue(other.NetworkId, out var sentRole) && sentRole == role)
                        continue;

                    player.SentRoles[other.NetworkId] = role;
                    other.Connection.Send(new RoleSyncInfo(player.ReferenceHub, role, other.ReferenceHub));
                }
            }
        }

        [LoaderInitialize(2)]
        private static void Init()
        {
            PlayerLoopHelper.ModifySystem(x =>
                x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(RoleSyncUpdateLoop)) ? x : null);
        }
    }
}