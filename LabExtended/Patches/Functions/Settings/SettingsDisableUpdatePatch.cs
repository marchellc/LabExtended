using HarmonyLib;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    public static class SettingsDisableUpdatePatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.UpdateDefinedSettings))]
        public static bool Prefix() => false;
    }
}