using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RoleSync;

using Mirror;

using PlayerRoles;

namespace LabExtended.Patches.Functions.Players
{
    public static class ProcessRoleOnJoinPatch
    {
        [HarmonyPatch(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack.WritePlayers))]
        public static bool Prefix(RoleSyncInfoPack __instance, NetworkWriter writer)
        {
            if (__instance._receiverHub is null
                || !ExPlayer.TryGet(__instance._receiverHub, out var receiver))
                return true;
            
            RoleManager.ProcessJoin(receiver, writer);
            return false;
        }
    }
}
