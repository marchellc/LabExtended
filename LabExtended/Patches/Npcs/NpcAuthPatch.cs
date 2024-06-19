using CentralAuth;

using HarmonyLib;

using LabExtended.API.Npcs;

namespace LabExtended.Patches.Npcs
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FixedUpdate))]
    public static class NpcAuthPatch
    {
        public static bool Prefix(PlayerAuthenticationManager __instance)
            => !__instance._hubSet || !NpcHandler.IsNpc(__instance._hub);
    }
}