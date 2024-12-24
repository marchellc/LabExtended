using HarmonyLib;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    public static class SettingsDisableSendPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
        public static bool SendToAllPrefix() => false;

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
        public static bool SendToSpecificHubPrefix() => false;

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
        public static bool SendToSpecificHubWithSettingsPrefix() => false;
        
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        public static bool SendToConditionallyPrefix() => false;
    }
}
