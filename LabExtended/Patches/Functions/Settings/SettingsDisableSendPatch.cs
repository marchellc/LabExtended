using HarmonyLib;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    public static class SettingsDisableSendPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer))]
        public static bool SendToAllPrefix() => false;

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer))]
        public static bool SendToSpecificPrefix() => false;

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        public static bool SendToConditionallyPrefix() => false;
    }
}
