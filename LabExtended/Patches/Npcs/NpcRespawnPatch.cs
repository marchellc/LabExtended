using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Npcs;

using PlayerRoles.Spectating;

namespace LabExtended.Patches.Npcs
{
    [HarmonyPatch(typeof(SpectatorRole), nameof(SpectatorRole.ReadyToRespawn), MethodType.Getter)]
    public static class NpcRespawnPatch
    {
        public static bool Prefix(SpectatorRole __instance, ref bool __result)
        {
            if (__instance.TryGetOwner(out var owner) && (NpcHandler.IsNpc(owner) || !(ExPlayer.Get(owner)?.CanRespawn ?? true)))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
