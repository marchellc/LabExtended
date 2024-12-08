using HarmonyLib;

using LabExtended.API;

using Mirror;

using PlayerRoles;

namespace LabExtended.Patches.Functions.SpectatorList
{
    public static class SpectatorListSyncRoleJoinPatch
    {
        [HarmonyPatch(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack.WritePlayers))]
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
                var fakedRole = player.InternalGetRoleForJoinedPlayer(receiver);

                if (player.Role.Role is IObfuscatedRole obfuscatedRole)
                    sentRole = obfuscatedRole.GetRoleForUser(receiver.Hub);

                if (fakedRole.HasValue)
                    sentRole = fakedRole.Value;

                new RoleSyncInfo(player.Hub, sentRole, receiver.Hub).Write(writer);

                player._sentRoles[receiver.PlayerId] = sentRole;
            }

            return false;
        }
    }
}
