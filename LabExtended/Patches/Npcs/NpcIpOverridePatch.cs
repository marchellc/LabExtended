using HarmonyLib;

using LabExtended.API.Npcs;

namespace LabExtended.Patches.Npcs
{
    [HarmonyPatch(typeof(PlayerIpOverride), nameof(PlayerIpOverride.Start))]
    public static class NpcIpOverridePatch
    {
        public static bool Prefix(PlayerIpOverride __instance)
            => !NpcHandler.Npcs.Any(npc => npc.Connection.connectionId == __instance.connectionToClient.connectionId);
    }
}