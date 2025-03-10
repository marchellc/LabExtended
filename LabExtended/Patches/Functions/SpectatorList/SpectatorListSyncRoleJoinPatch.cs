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

            writer.WriteUShort((ushort)ExPlayer.AllPlayers.Count);

            foreach (var player in ExPlayer.AllPlayers)
            {
                var sentRole = player.Role.Type;
                var fakedRole = !player.Toggles.IsVisibleInSpectatorList ? RoleTypeId.Spectator : RoleTypeId.None;

                if (player.Role.Role is IObfuscatedRole obfuscatedRole) sentRole = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);
                if (fakedRole != RoleTypeId.None) sentRole = fakedRole;

                new RoleSyncInfo(player.ReferenceHub, sentRole, receiver.ReferenceHub).Write(writer);

                player.SentRoles[receiver.NetworkId] = sentRole;
            }

            return false;
        }
    }
}
