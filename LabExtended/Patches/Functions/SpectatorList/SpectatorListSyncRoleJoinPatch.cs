using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;

using Mirror;

using PlayerRoles;

namespace LabExtended.Patches.Functions.SpectatorList
{
    [HarmonyPatch(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack.WritePlayers))]
    public static class SpectatorListSyncRoleJoinPatch
    {
        public static bool Prefix(RoleSyncInfoPack __instance, NetworkWriter writer)
        {
            if (__instance._receiverHub is null)
                return true;

            var receiver = ExPlayer.Get(__instance._receiverHub);

            if (receiver is null)
                return true;

            writer.WriteUShort((ushort)ExPlayer._allPlayers.Count);

            foreach (var player in ExPlayer._allPlayers)
            {
                var sentRole = player.Role.Type;
                var fakedRole = player.GetRoleForJoinedPlayer(receiver);

                if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                    sentRole = obfuscatedRole.GetRoleForUser(receiver.Hub);

                if (fakedRole.HasValue)
                    sentRole = fakedRole.Value;

                ExLoader.Debug("Player API", $"Setting initial role of &3{player.Name}&r (&6{player.UserId}&r) to &1{sentRole}&r (for &3{receiver.Name}&r &6{receiver.UserId}&r)");

                new RoleSyncInfo(player.Hub, sentRole, receiver.Hub).Write(writer);

                player._sentRoles[receiver.PlayerId] = sentRole;
            }

            return false;
        }
    }
}
